using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;
using System.Text;
using TradeWeb.API.Catalog;
using TradeWeb.API.Infrastracture;
using TradeWeb.API.Models;

namespace TradeWeb.API.Services;

public sealed class TradeEnrichmentService : ITradeEnrichmentService
{
    private static readonly byte[] OutputHeader = "date,productName,currency,price\n"u8.ToArray();

    private readonly IProductCatalog _catalog;
    private readonly MissingMappingTracker _missingTracker;
    private readonly ILogger<TradeEnrichmentService> _logger;

    private const int MaxInvalidRowLogs = 1000;
    private int _invalidRowLogCount = 0;


    public TradeEnrichmentService(
        IProductCatalog catalog,
        MissingMappingTracker missingTracker,
        ILogger<TradeEnrichmentService> logger)
    {
        _catalog = catalog;
        _missingTracker = missingTracker;
        _logger = logger;
    }

    public async Task EnrichAsync(Stream csvInput, PipeWriter csvOutput, CancellationToken ct)
    {
        var reader = PipeReader.Create(csvInput, new StreamPipeReaderOptions(bufferSize: 1 << 16));

        bool headerSkipped = false;
        long lineNo = 0;

        await foreach (var lineMem in CsvLineReader.ReadLinesAsync(reader, ct))
        {
            ct.ThrowIfCancellationRequested();
            lineNo++;

            var trimmed = TrimCr(lineMem);
            if (trimmed.Length == 0) continue;

            if (!headerSkipped)
            {
                headerSkipped = true;
                if (StartsWithHeader(trimmed.Span))
                    continue;
            }

            if (!TrySplit4(trimmed.Span, out var f))    // f is a struct (OK in async)
            {
                LogInvalidRow(lineNo, "Wrong column count", trimmed.Span);
                continue;
            }

            if (!DateValidator.IsValidYyyyMmDd(trimmed.Span.Slice(f.DateStart, f.DateLen)))
            {
                LogInvalidRow(lineNo, "Invalid date yyyyMMdd", trimmed.Span);
                continue;
            }

            if (!Utf8Parser.TryParse(
                    trimmed.Span.Slice(f.ProductIdStart, f.ProductIdLen),
                    out int productId,
                    out int consumed)
                || consumed != f.ProductIdLen)
            {
                LogInvalidRow(lineNo, "Invalid productId", trimmed.Span);
                continue;
            }

            if (!_catalog.TryGetProductNameUtf8(productId, out var productNameUtf8))
            {
                if (_missingTracker.ShouldLog(productId))
                    _logger.LogWarning("Missing product mapping for productId={ProductId}", productId);

                productNameUtf8 = _catalog.MissingProductNameCsvUtf8;
            }

            csvOutput.Write(trimmed.Span.Slice(f.DateStart, f.DateLen));
            csvOutput.Write(","u8);
            csvOutput.Write(productNameUtf8.Span);
            csvOutput.Write(","u8);
            csvOutput.Write(trimmed.Span.Slice(f.CurrencyStart, f.CurrencyLen));
            csvOutput.Write(","u8);
            csvOutput.Write(trimmed.Span.Slice(f.PriceStart, f.PriceLen));
            csvOutput.Write("\n"u8);
        }

        await csvOutput.FlushAsync(ct);
        await csvOutput.CompleteAsync();
    }

    private void LogInvalidRow(long lineNo, string reason, ReadOnlySpan<byte> line)
    {
        var n = Interlocked.Increment(ref _invalidRowLogCount);
        if (n <= MaxInvalidRowLogs)
        {
            _logger.LogError("Discarding trade row line {LineNo}: {Reason}. Row={Row}",
                lineNo, reason, Encoding.UTF8.GetString(line));
        }
        else if (n == MaxInvalidRowLogs + 1)
        {
            _logger.LogError("Too many invalid rows; suppressing further invalid-row logs.");
        }
    }

    private static ReadOnlyMemory<byte> TrimCr(ReadOnlyMemory<byte> m)
    {
        if (m.Length == 0) return m;
        return m.Span[^1] == (byte)'\r' ? m[..^1] : m;
    }

    private static bool StartsWithHeader(ReadOnlySpan<byte> line)
        => line.StartsWith("date,"u8);

    private static bool TrySplit4(ReadOnlySpan<byte> line, out TradeFields f)
    {
        f = default;

        int p1 = line.IndexOf((byte)',');
        if (p1 < 0) return false;

        int p2rel = line[(p1 + 1)..].IndexOf((byte)',');
        if (p2rel < 0) return false;
        int p2 = p1 + 1 + p2rel;

        int p3rel = line[(p2 + 1)..].IndexOf((byte)',');
        if (p3rel < 0) return false;
        int p3 = p2 + 1 + p3rel;

        // date
        int dS = 0, dL = p1;
        // productId
        int idS = p1 + 1, idL = p2 - idS;
        // currency
        int cS = p2 + 1, cL = p3 - cS;
        // price
        int prS = p3 + 1, prL = line.Length - prS;

        if (dL <= 0 || idL <= 0 || cL <= 0 || prL <= 0) return false;

        f = new TradeFields(dS, dL, idS, idL, cS, cL, prS, prL);
        return true;
    }
}

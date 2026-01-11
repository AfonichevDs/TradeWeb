using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using System.Text;
using TradeWeb.Application.Interfaces;
using TradeWeb.Infrastructure.Helpers;
using TradeWeb.Infrastructure.Processing;
using TradeWeb.Infrastructure.Processing.Csv;

namespace TradeWeb.Tests;
public class TradeEnrichmentServiceTests
{
    [Fact]
    public async Task Skips_Invalid_Date_Rows_And_Logs()
    {
        var catalog = BuildCatalog((1, "A"));
        var missingTracker = new MissingMappingTracker();
        var loggerProvider = new ListLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(loggerProvider));
        var logger = loggerFactory.CreateLogger<TradeEnrichmentService>();

        var svc = new TradeEnrichmentService(catalog, missingTracker, logger);

        var input = """
date,productId,currency,price
20250230,1,EUR,10
20250228,1,EUR,20
""";

        await using var inStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var outStream = new MemoryStream();
        var writer = PipeWriter.Create(outStream);

        await svc.EnrichAsync(inStream, writer, CancellationToken.None);

        await writer.FlushAsync();
        await writer.CompleteAsync();

        var output = Encoding.UTF8.GetString(outStream.ToArray());
        Assert.Contains("20250228,A,EUR,20", output);
        Assert.DoesNotContain("20250230", output);

        Assert.Contains(loggerProvider.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("Invalid date"));
    }

    [Fact]
    public async Task Missing_ProductId_Outputs_Missing_Name_And_Logs_Once()
    {
        var catalog = BuildCatalog((1, "A"));
        var missingTracker = new MissingMappingTracker();
        var loggerProvider = new ListLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(loggerProvider));
        var logger = loggerFactory.CreateLogger<TradeEnrichmentService>();

        var svc = new TradeEnrichmentService(catalog, missingTracker, logger);

        var input = """
            date,productId,currency,price
            20250101,999,EUR,10
            20250102,999,EUR,20
            """;

        await using var inStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var outStream = new MemoryStream();
        var writer = PipeWriter.Create(outStream);

        await svc.EnrichAsync(inStream, writer, CancellationToken.None);

        var output = Encoding.UTF8.GetString(outStream.ToArray());
        Assert.Contains("20250101,Missing Product Name,EUR,10", output);
        Assert.Contains("20250102,Missing Product Name,EUR,20", output);

        var warnings = loggerProvider.Entries.Where(e => e.Level == LogLevel.Warning && e.Message.Contains("Missing product mapping")).ToList();
        Assert.Single(warnings);
    }

    private static IProductCatalog BuildCatalog(params (int id, string name)[] items)
    {
        var dict = items.ToDictionary(x => x.id, x => (ReadOnlyMemory<byte>)CsvEscaper.ToCsvFieldUtf8(x.name));
        var map = System.Collections.Immutable.ImmutableDictionary.CreateRange(dict);
        var missing = (ReadOnlyMemory<byte>)CsvEscaper.ToCsvFieldUtf8("Missing Product Name");
        return new FakeCatalog(map, missing);

        static ReadOnlyMemory<byte> Bytes(string s) => Encoding.UTF8.GetBytes(s);

    }
    sealed class FakeCatalog : IProductCatalog
    {
        private readonly System.Collections.Immutable.ImmutableDictionary<int, ReadOnlyMemory<byte>> _map;
        public ReadOnlyMemory<byte> MissingProductNameCsvUtf8 { get; }
        public FakeCatalog(System.Collections.Immutable.ImmutableDictionary<int, ReadOnlyMemory<byte>> map, ReadOnlyMemory<byte> missing)
        { _map = map; MissingProductNameCsvUtf8 = missing; }

        public bool TryGetProductNameUtf8(int productId, out ReadOnlyMemory<byte> productNameCsvUtf8)
            => _map.TryGetValue(productId, out productNameCsvUtf8);
    }
}
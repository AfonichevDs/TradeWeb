using System.Collections.Immutable;
using System.Text;
using TradeWeb.API.Infrastracture;

namespace TradeWeb.API.Catalog;

public class ProductCatalog: IProductCatalog
{
    private readonly ImmutableDictionary<int, ReadOnlyMemory<byte>> _map;
    public ReadOnlyMemory<byte> MissingProductNameCsvUtf8 { get; }

    private ProductCatalog(
    ImmutableDictionary<int, ReadOnlyMemory<byte>> map,
    ReadOnlyMemory<byte> missing)
    {
        _map = map;
        MissingProductNameCsvUtf8 = missing;
    }

    public bool TryGetProductNameUtf8(int productId, out ReadOnlyMemory<byte> productNameCsvUtf8)
    => _map.TryGetValue(productId, out productNameCsvUtf8);

    public static ProductCatalog LoadFromCsv(string path, ILogger logger)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Product CSV not found: {path}");

        var dict = ImmutableDictionary.CreateBuilder<int, ReadOnlyMemory<byte>>();

        using var fs = File.OpenRead(path);
        using var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1 << 16);

        string? line;
        int lineNo = 0;
        while ((line = sr.ReadLine()) is not null)
        {
            lineNo++;
            if (lineNo == 1 && line.StartsWith("productId", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var comma = line.IndexOf(',');
            if (comma <= 0 || comma == line.Length - 1)
            {
                logger.LogWarning("Invalid product row at line {LineNo}: {Line}", lineNo, line);
                continue;
            }

            var idPart = line.AsSpan(0, comma).Trim();
            var namePart = line.AsSpan(comma + 1).Trim();

            if (!int.TryParse(idPart, out var id))
            {
                logger.LogWarning("Invalid productId at line {LineNo}: {Line}", lineNo, line);
                continue;
            }

            var name = namePart.ToString();
            var csvBytes = CsvEscaper.ToCsvFieldUtf8(name);

            dict[id] = csvBytes;
        }

        var missing = CsvEscaper.ToCsvFieldUtf8("Missing Product Name");
        logger.LogInformation("Loaded {Count} products from {Path}", dict.Count, path);

        return new ProductCatalog(dict.ToImmutable(), missing);
    }
}

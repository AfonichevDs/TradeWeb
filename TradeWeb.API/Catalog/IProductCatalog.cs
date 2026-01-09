namespace TradeWeb.API.Catalog;

public interface IProductCatalog
{
    bool TryGetProductNameUtf8(int productId, out ReadOnlyMemory<byte> productNameCsvUtf8);

    ReadOnlyMemory<byte> MissingProductNameCsvUtf8 { get; }
}

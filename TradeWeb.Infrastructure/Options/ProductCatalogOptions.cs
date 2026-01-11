namespace TradeWeb.Infrastructure.Options;
public class ProductCatalogOptions
{
    public const string SectionName = "ProductCatalog";

    public string ProductCsvPath { get; init; } = "";
}

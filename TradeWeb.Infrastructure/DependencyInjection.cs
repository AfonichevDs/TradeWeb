using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeWeb.Application.Interfaces;
using TradeWeb.Infrastructure.Helpers;
using TradeWeb.Infrastructure.Options;
using TradeWeb.Infrastructure.Processing;
using TradeWeb.Infrastructure.Services;

namespace TradeWeb.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ProductCatalogOptions>()
            .Bind(configuration.GetSection(ProductCatalogOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ProductCsvPath), "ProductCsvPath is required")
            .ValidateOnStart();

        services.AddSingleton<MissingMappingTracker>();

        services.AddSingleton<IProductCatalog>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<ProductCatalogOptions>>().Value;
            var env = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var logger = sp.GetRequiredService<ILogger<ProductCatalog>>();

            var fullPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, opt.ProductCsvPath));
            return ProductCatalog.LoadFromCsv(fullPath, logger);
        });

        services.AddScoped<ITradeEnrichmentService, TradeEnrichmentService>();

        return services;
    }
}
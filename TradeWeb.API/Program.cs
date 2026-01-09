using Microsoft.Extensions.Options;
using TradeWeb.API.Catalog;
using TradeWeb.API.Infrastracture;
using TradeWeb.API.Options;
using TradeWeb.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOptions<ProductCatalogOptions>()
    .Bind(builder.Configuration.GetSection(ProductCatalogOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ProductCsvPath), "ProductCsvPath is required")
    .ValidateOnStart();

builder.Services.AddSingleton<IProductCatalog>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<ProductCatalogOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<ProductCatalog>>();
    var env = sp.GetRequiredService<IHostEnvironment>();

    var fullPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, opt.ProductCsvPath));

    return ProductCatalog.LoadFromCsv(fullPath, logger);
});

builder.Services.AddSingleton<MissingMappingTracker>();
builder.Services.AddScoped<ITradeEnrichmentService, TradeEnrichmentService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

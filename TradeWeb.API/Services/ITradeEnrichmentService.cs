using System.IO.Pipelines;

namespace TradeWeb.API.Services;

public interface ITradeEnrichmentService
{
    Task EnrichAsync(Stream csvInput, PipeWriter csvOutput, CancellationToken ct);
}

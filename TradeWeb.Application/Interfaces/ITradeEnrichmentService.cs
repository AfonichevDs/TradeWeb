using System.IO.Pipelines;

namespace TradeWeb.Application.Interfaces;
public interface ITradeEnrichmentService
{
    Task EnrichAsync(Stream csvInput, PipeWriter csvOutput, CancellationToken ct);
}


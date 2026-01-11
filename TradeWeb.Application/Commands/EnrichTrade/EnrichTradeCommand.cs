using MediatR;
using System.IO.Pipelines;
using TradeWeb.Application.Interfaces;

namespace TradeWeb.Application.Commands.EnrichTrade;
public record EnrichTradeCommand(
    Stream InputCsv,
    PipeWriter OutputCsv,
    string? TraceId = null
) : IRequest;

public class EnrichTradesHandler : IRequestHandler<EnrichTradeCommand>
{
    private readonly ITradeEnrichmentService _processor;

    public EnrichTradesHandler(ITradeEnrichmentService processor)
    {
        _processor = processor;
    }

    public Task Handle(EnrichTradeCommand request, CancellationToken ct)
        => _processor.EnrichAsync(request.InputCsv, request.OutputCsv, ct);
}
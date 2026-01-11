using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TradeWeb.Application.Commands.EnrichTrade;

namespace TradeWeb.API.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class TradeEnrichmentController(ILogger<TradeEnrichmentController> _logger, IMediator _mediator) : ControllerBase
{
    [DisableRequestSizeLimit]
    [HttpPost("enrich")]
    [Consumes("text/csv")]
    [Produces("text/csv")]
    public async Task<IActionResult> Enrich(CancellationToken ct)
    {
        if (Request.ContentType is null ||
            !Request.ContentType.StartsWith("text/csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Content-Type must be text/csv");
        }

        Response.ContentType = "text/csv; charset=utf-8";

        var sw = Stopwatch.StartNew();

        await _mediator.Send(
            new EnrichTradeCommand(Request.Body, Response.BodyWriter, HttpContext.TraceIdentifier),
            ct);

        sw.Stop();
        _logger.LogInformation(
            "Trade enrichment finished in {ElapsedMs} ms. TraceId={TraceId}",
            sw.ElapsedMilliseconds,
            HttpContext.TraceIdentifier);

        return new EmptyResult();
    }
}
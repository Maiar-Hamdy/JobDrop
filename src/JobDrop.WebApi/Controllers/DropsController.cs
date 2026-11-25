using JobDrop.Application.Features.Drops;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class DropsController(ISender mediator) : ControllerBase
{
    [HttpPost("jobs/{jobId:guid}/drop-preview")]
    public async Task<IActionResult> Preview(Guid jobId, CancellationToken ct)
        => Ok(await mediator.Send(new DropPreviewQuery(jobId), ct));

    [HttpPost("jobs/{jobId:guid}/drops")]
    public async Task<IActionResult> Dispatch(Guid jobId,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromBody] DispatchDropBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return BadRequest(new { code = "MISSING_IDEMPOTENCY_KEY" });
        var result = await mediator.Send(new DispatchDropCommand(jobId, idempotencyKey, body.ResumeDocumentId, body.CoverNote), ct);
        return result.IsIdempotentReplay ? Ok(result) : StatusCode(201, result);
    }

    [HttpGet("me/drops")]
    public async Task<IActionResult> List([FromQuery] DropTab tab = DropTab.All,
        [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetMyDropsListQuery(tab, search, page, pageSize), ct));

    [HttpGet("me/drops/summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Ok(await mediator.Send(new GetMyDropsSummaryQuery(), ct));

    [HttpPut("me/pinned-drops")]
    public async Task<IActionResult> Pin([FromBody] PinDropsBody body, CancellationToken ct)
    { await mediator.Send(new PinDropsCommand(body.ApplicationIds), ct); return NoContent(); }
}

public sealed record DispatchDropBody(Guid? ResumeDocumentId, string? CoverNote);
public sealed record PinDropsBody(IReadOnlyList<Guid> ApplicationIds);
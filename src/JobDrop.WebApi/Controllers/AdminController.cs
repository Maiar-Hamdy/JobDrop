using JobDrop.Application.Features.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "PlatformAdmin")]
public sealed class AdminController(ISender mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await mediator.Send(new GetAdminDashboardQuery(), ct));

    [HttpPost("reviews/{reviewId:guid}/moderation")]
    public async Task<IActionResult> Moderate(Guid reviewId, [FromBody] ModerationBody body, CancellationToken ct)
    { await mediator.Send(new ModerationDecisionCmd(reviewId, body.Decision), ct); return NoContent(); }

    [HttpPost("notifications/replay-dlq")]
    public async Task<IActionResult> ReplayDlq(CancellationToken ct)
        => Ok(new { replayed = await mediator.Send(new ReplayDlqCmd(), ct) });
}

public sealed record ModerationBody(string Decision);
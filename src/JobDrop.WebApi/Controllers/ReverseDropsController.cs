using JobDrop.Application.Features.ReverseDrops;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/reverse-drops")]
[Authorize]
public sealed class ReverseDropsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Dispatch([FromHeader(Name = "Idempotency-Key")] string key,
        [FromBody] DispatchReverseDropCmd cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return BadRequest(new { code = "MISSING_IDEMPOTENCY_KEY" });
        return Created($"/api/v1/reverse-drops/{await mediator.Send(cmd with { IdempotencyKey = key }, ct)}", new { });
    }

    [HttpPost("{id:guid}/{action}")]
    public async Task<IActionResult> Respond(Guid id, string action, CancellationToken ct)
    { await mediator.Send(new RespondReverseDropCmd(id, action), ct); return NoContent(); }
}
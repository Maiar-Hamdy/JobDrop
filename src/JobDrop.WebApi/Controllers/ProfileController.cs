using JobDrop.Application.Features.Candidates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize]
public sealed class ProfileController(ISender mediator) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetProfileQuery(), ct));

    [HttpPatch("profile")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileCommand cmd, CancellationToken ct)
    { await mediator.Send(cmd, ct); return NoContent(); }

    [HttpGet("profile-completeness")]
    public async Task<IActionResult> Completeness(CancellationToken ct)
        => Ok(await mediator.Send(new GetCompletenessQuery(), ct));

    [HttpGet("starting-points")]
    public async Task<IActionResult> ListSp(CancellationToken ct)
        => Ok(await mediator.Send(new ListStartingPointsQuery(), ct));

    [HttpPost("starting-points")]
    public async Task<IActionResult> AddSp([FromBody] AddStartingPointCmd cmd, CancellationToken ct)
        => Ok(new { id = await mediator.Send(cmd, ct) });

    [HttpDelete("starting-points/{id:guid}")]
    public async Task<IActionResult> DelSp(Guid id, CancellationToken ct)
    { await mediator.Send(new DeleteStartingPointCmd(id), ct); return NoContent(); }
}
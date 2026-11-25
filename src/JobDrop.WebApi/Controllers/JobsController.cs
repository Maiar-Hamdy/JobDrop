using JobDrop.Application.Features.Jobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class JobsController(ISender mediator) : ControllerBase
{
    [HttpPost("companies/{companyId:guid}/jobs")]
    public async Task<IActionResult> Create(Guid companyId, [FromBody] CreateJobCmd cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd with { CompanyId = companyId }, ct);
        return Created($"/api/v1/jobs/{id}", new { id });
    }

    [HttpGet("jobs/{jobId:guid}")]
    public async Task<IActionResult> Get(Guid jobId, CancellationToken ct)
        => Ok(await mediator.Send(new GetJobQuery(jobId), ct));

    [HttpPost("jobs/{jobId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid jobId, CancellationToken ct)
    { await mediator.Send(new JobLifecycleCmd(jobId, "publish"), ct); return NoContent(); }

    [HttpPost("jobs/{jobId:guid}/pause-intake")]
    public async Task<IActionResult> Pause(Guid jobId, CancellationToken ct)
    { await mediator.Send(new JobLifecycleCmd(jobId, "pause-intake"), ct); return NoContent(); }

    [HttpPost("jobs/{jobId:guid}/resume-intake")]
    public async Task<IActionResult> Resume(Guid jobId, CancellationToken ct)
    { await mediator.Send(new JobLifecycleCmd(jobId, "resume-intake"), ct); return NoContent(); }

    [HttpPost("jobs/{jobId:guid}/close")]
    public async Task<IActionResult> Close(Guid jobId, CancellationToken ct)
    { await mediator.Send(new JobLifecycleCmd(jobId, "close"), ct); return NoContent(); }

    [HttpPost("jobs/{jobId:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid jobId, CancellationToken ct)
    { await mediator.Send(new JobLifecycleCmd(jobId, "reopen"), ct); return NoContent(); }

    [HttpPut("jobs/{jobId:guid}/intake-policy")]
    public async Task<IActionResult> Intake(Guid jobId, [FromBody] UpdateIntakePolicyCmd cmd, CancellationToken ct)
    { await mediator.Send(cmd with { JobId = jobId }, ct); return NoContent(); }
}
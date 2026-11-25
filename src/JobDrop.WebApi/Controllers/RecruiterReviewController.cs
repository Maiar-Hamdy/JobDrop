using JobDrop.Application.Features.RecruiterReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class RecruiterReviewController(ISender mediator) : ControllerBase
{
    [HttpGet("drops/{applicationId:guid}/dossier")]
    public async Task<IActionResult> GetDossier(Guid applicationId, CancellationToken ct)
        => Ok(await mediator.Send(new GetDossierQuery(applicationId), ct));

    [HttpPost("drops/{applicationId:guid}/open")]
    public async Task<IActionResult> Open(Guid applicationId, CancellationToken ct)
    { await mediator.Send(new DossierActionCmd(applicationId, "open", null, null), ct); return NoContent(); }

    [HttpPost("drops/{applicationId:guid}/shortlist")]
    public async Task<IActionResult> Shortlist(Guid applicationId, CancellationToken ct)
    { await mediator.Send(new DossierActionCmd(applicationId, "shortlist", null, null), ct); return NoContent(); }

    [HttpPost("drops/{applicationId:guid}/invite-interview")]
    public async Task<IActionResult> Invite(Guid applicationId, CancellationToken ct)
    { await mediator.Send(new DossierActionCmd(applicationId, "invite-interview", null, null), ct); return NoContent(); }

    [HttpPost("drops/{applicationId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid applicationId, [FromBody] RejectBody body, CancellationToken ct)
    { await mediator.Send(new DossierActionCmd(applicationId, "reject", body.RejectionReasonCode, body.Note), ct); return NoContent(); }
}

public sealed record RejectBody(string RejectionReasonCode, string? Note);
using JobDrop.Application.Features.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class ReviewsController(ISender mediator) : ControllerBase
{
    [HttpPost("drops/{applicationId:guid}/reviews")]
    public async Task<IActionResult> Submit(Guid applicationId, [FromBody] SubmitReviewBody body, CancellationToken ct)
        => Ok(new { id = await mediator.Send(new SubmitReviewCmd(applicationId, body.Rating, body.Comment, body.TargetType), ct) });

    [HttpPost("reviews/{reviewId:guid}/reports")]
    public async Task<IActionResult> Report(Guid reviewId, [FromBody] ReportReviewBody body, CancellationToken ct)
        => Ok(new { id = await mediator.Send(new ReportReviewCmd(reviewId, body.Reason), ct) });
}

public sealed record SubmitReviewBody(int Rating, string? Comment, string TargetType);
public sealed record ReportReviewBody(string Reason);
using JobDrop.Application.Features.Insights;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/insights")]
[Authorize]
public sealed class InsightsController(ISender mediator) : ControllerBase
{
    [HttpGet("personal")]
    public async Task<IActionResult> Personal([FromQuery] string period = "all", CancellationToken ct = default)
        => Ok(await mediator.Send(new GetPersonalInsightsQuery(period), ct));
}
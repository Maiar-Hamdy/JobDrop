using JobDrop.Application.Features.Discovery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/discovery")]
[Authorize]
public sealed class DiscoveryController(ISender mediator) : ControllerBase
{
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] DiscoveryFilter filter, CancellationToken ct)
        => Ok(await mediator.Send(new DiscoverySearchCommand(filter), ct));
}
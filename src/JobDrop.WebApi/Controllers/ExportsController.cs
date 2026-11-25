using JobDrop.Application.Features.Exports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/exports")]
[Authorize]
public sealed class ExportsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestExportCmd cmd, CancellationToken ct)
        => Accepted(new { id = await mediator.Send(cmd, ct) });

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Status(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetExportStatusQuery(id), ct));
}
using JobDrop.Application.Features.Companies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/companies")]
[Authorize]
public sealed class CompaniesController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyCmd cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return Created($"/api/v1/companies/{id}", new { id });
    }

    [HttpPost("{companyId:guid}/offices")]
    public async Task<IActionResult> AddOffice(Guid companyId, [FromBody] CreateOfficeCmd body, CancellationToken ct)
    {
        var id = await mediator.Send(body with { CompanyId = companyId }, ct);
        return Ok(new { id });
    }

    [HttpGet("{companyId:guid}/offices")]
    public async Task<IActionResult> ListOffices(Guid companyId, CancellationToken ct)
        => Ok(await mediator.Send(new ListOfficesQuery(companyId), ct));

    [HttpGet("{companyId:guid}/calendar")]
    public async Task<IActionResult> GetCalendar(Guid companyId, CancellationToken ct)
        => Ok(await mediator.Send(new GetCalendarQuery(companyId), ct));

    [HttpPut("{companyId:guid}/calendar")]
    public async Task<IActionResult> UpsertCalendar(Guid companyId, [FromBody] UpsertCalendarCmd body, CancellationToken ct)
    { await mediator.Send(body with { CompanyId = companyId }, ct); return NoContent(); }
}
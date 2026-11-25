using JobDrop.Application.Features.Drops;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/me/wallets")]
[Authorize]
public sealed class WalletController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetWalletQuery(), ct));

    [HttpGet("ledger")]
    public async Task<IActionResult> Ledger([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetLedgerQuery(take), ct));
}
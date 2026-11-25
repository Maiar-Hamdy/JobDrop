using JobDrop.Application.Features.Billing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/billing")]
public sealed class BillingController(ISender mediator) : ControllerBase
{
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> Plans(CancellationToken ct)
        => Ok(await mediator.Send(new ListPlansQuery(), ct));

    [HttpPost("checkouts")]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CreateCheckoutCmd cmd, CancellationToken ct)
        => Ok(await mediator.Send(cmd, ct));

    [HttpGet("entitlements")]
    [Authorize]
    public async Task<IActionResult> Entitlements(CancellationToken ct)
        => Ok(await mediator.Send(new GetEntitlementsQuery(), ct));

    [HttpPost("webhooks/paymob")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobWebhook([FromBody] PaymobWebhookBody body, CancellationToken ct)
    {
        await mediator.Send(new PaymobWebhookCmd(body.RawPayload, body.Hmac), ct);
        return Ok();
    }
}

public sealed record PaymobWebhookBody(string RawPayload, string Hmac);
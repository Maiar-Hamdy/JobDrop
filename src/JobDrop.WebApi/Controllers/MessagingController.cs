using JobDrop.Application.Features.Messaging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Controllers;

[ApiController]
[Route("api/v1/conversations")]
[Authorize]
public sealed class MessagingController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await mediator.Send(new ListConversationsQuery(), ct));

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> Messages(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new ListMessagesQuery(id), ct));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> Send(Guid id, [FromBody] SendMessageCmd body, CancellationToken ct)
        => Ok(new { id = await mediator.Send(body with { ConversationId = id }, ct) });

    [HttpPost("{id:guid}/interview-proposals")]
    public async Task<IActionResult> Propose(Guid id, [FromBody] ProposeInterviewCmd body, CancellationToken ct)
        => Ok(new { id = await mediator.Send(body with { ConversationId = id }, ct) });

    [HttpPost("for-application/{applicationId:guid}")]
    public async Task<IActionResult> OpenForApp(Guid applicationId, CancellationToken ct)
        => Ok(new { id = await mediator.Send(new OpenConversationForApplicationCmd(applicationId), ct) });
}
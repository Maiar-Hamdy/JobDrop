using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Messaging;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCmd>
{
    public SendMessageValidator() => RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
}

public sealed class SendMessageHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<SendMessageCmd, Guid>
{
    public async Task<Guid> Handle(SendMessageCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == r.ConversationId, ct)
            ?? throw new Domain.Common.NotFoundException("Conversation not found.");
        conv.LastMessageAtUtc = clock.UtcNow;
        var msg = new Message { ConversationId = conv.Id, SenderUserId = uid, Body = r.Body, CreatedAtUtc = clock.UtcNow };
        db.Messages.Add(msg);
        await db.SaveChangesAsync(ct);
        return msg.Id;
    }
}

public sealed class ListConversationsHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<ListConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(ListConversationsQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        return await db.Conversations.AsNoTracking()
            .Where(c => c.CandidateProfileId == profileId)
            .OrderByDescending(c => c.LastMessageAtUtc)
            .Select(c => new ConversationDto(c.Id, c.ApplicationId, c.ReverseDropId, c.CompanyId, c.LastMessageAtUtc))
            .ToListAsync(ct);
    }
}

public sealed class ListMessagesHandler(IApplicationDbContext db)
    : IRequestHandler<ListMessagesQuery, IReadOnlyList<MessageDto>>
{
    public async Task<IReadOnlyList<MessageDto>> Handle(ListMessagesQuery r, CancellationToken ct)
        => await db.Messages.AsNoTracking()
            .Where(m => m.ConversationId == r.ConversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderUserId, m.Body, m.CreatedAtUtc))
            .ToListAsync(ct);
}

public sealed class OpenConversationForApplicationHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<OpenConversationForApplicationCmd, Guid>
{
    public async Task<Guid> Handle(OpenConversationForApplicationCmd r, CancellationToken ct)
    {
        var app = await db.Applications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == r.ApplicationId, ct)
            ?? throw new Domain.Common.NotFoundException("Application not found.");
        var existing = await db.Conversations.FirstOrDefaultAsync(c => c.ApplicationId == app.Id, ct);
        if (existing != null) return existing.Id;
        var conv = new Conversation
        {
            ApplicationId = app.Id,
            CompanyId = app.CompanyId,
            CandidateProfileId = app.CandidateProfileId,
            LastMessageAtUtc = clock.UtcNow
        };
        db.Conversations.Add(conv);
        await db.SaveChangesAsync(ct);
        return conv.Id;
    }
}

public sealed class ProposeInterviewValidator : AbstractValidator<ProposeInterviewCmd>
{
    public ProposeInterviewValidator()
    {
        RuleFor(x => x.CallType).Must(v => v is "Voice" or "Video");
        RuleFor(x => x.SlotsIso).NotEmpty();
    }
}

public sealed class ProposeInterviewHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<ProposeInterviewCmd, Guid>
{
    public async Task<Guid> Handle(ProposeInterviewCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var proposal = new InterviewProposal
        {
            ConversationId = r.ConversationId,
            ProposedByUserId = uid,
            CallType = r.CallType,
            SlotsJson = System.Text.Json.JsonSerializer.Serialize(r.SlotsIso),
            Status = "Proposed"
        };
        db.InterviewProposals.Add(proposal);
        await db.SaveChangesAsync(ct);
        return proposal.Id;
    }
}
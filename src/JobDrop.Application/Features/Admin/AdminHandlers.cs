using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Admin;

public sealed class GetAdminDashboardHandler(IApplicationDbContext db)
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery r, CancellationToken ct)
    {
        var users = await db.GetUserCountAsync(ct);
        var candidates = await db.CandidateProfiles.CountAsync(ct);
        var companies = await db.Companies.CountAsync(ct);
        var published = await db.Jobs.CountAsync(j => j.Status == JobStatus.Published, ct);
        var paused = await db.Jobs.CountAsync(j => j.Status == JobStatus.Paused, ct);
        var closed = await db.Jobs.CountAsync(j => j.Status == JobStatus.Closed, ct);
        var delivered = await db.Applications.CountAsync(a => a.Status == ApplicationStatus.Delivered, ct);
        var queued = await db.Applications.CountAsync(a => a.Status == ApplicationStatus.Queued, ct);
        var refunded = await db.Applications.CountAsync(a => a.Status == ApplicationStatus.ExpiredRefunded, ct);
        var subs = await db.Subscriptions.CountAsync(s => s.Status == "Active", ct);
        var pendingReviews = await db.Reviews.CountAsync(x => x.Status == "Pending", ct);
        var outbox = await db.OutboxMessages
            .CountAsync(m => m.DeliveredAtUtc == null && m.DeadLetteredAtUtc == null, ct);
        var dlq = await db.OutboxMessages.CountAsync(m => m.DeadLetteredAtUtc != null, ct);

        return new AdminDashboardDto(users, candidates, companies, published, paused, closed,
            delivered, queued, refunded, subs, 0, pendingReviews, outbox, dlq);
    }
}

public sealed class VerificationDecisionValidator : AbstractValidator<VerificationDecisionCmd>
{
    public VerificationDecisionValidator()
        => RuleFor(x => x.Decision).Must(d => d is "approve" or "reject");
}

public sealed class VerificationDecisionHandler : IRequestHandler<VerificationDecisionCmd, Unit>
{
    public Task<Unit> Handle(VerificationDecisionCmd r, CancellationToken ct) => Task.FromResult(Unit.Value);
}

public sealed class ModerationDecisionValidator : AbstractValidator<ModerationDecisionCmd>
{
    public ModerationDecisionValidator()
        => RuleFor(x => x.Decision).Must(d => d is "Approved" or "Rejected" or "Hidden");
}

public sealed class ModerationDecisionHandler(IApplicationDbContext db)
    : IRequestHandler<ModerationDecisionCmd, Unit>
{
    public async Task<Unit> Handle(ModerationDecisionCmd r, CancellationToken ct)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(x => x.Id == r.ReviewId, ct)
            ?? throw new Domain.Common.NotFoundException("Review not found.");
        review.Status = r.Decision;
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public sealed class ReplayDlqHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<ReplayDlqCmd, int>
{
    public async Task<int> Handle(ReplayDlqCmd r, CancellationToken ct)
    {
        var dead = await db.OutboxMessages
            .Where(m => m.DeadLetteredAtUtc != null).Take(100).ToListAsync(ct);
        foreach (var m in dead)
        {
            m.DeadLetteredAtUtc = null;
            m.RetryCount = 0;
            m.NextAttemptAtUtc = clock.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return dead.Count;
    }
}
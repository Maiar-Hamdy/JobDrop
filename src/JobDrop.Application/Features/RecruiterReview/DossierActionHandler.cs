using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Drops;
using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.RecruiterReview;

public sealed class DossierActionValidator : AbstractValidator<DossierActionCmd>
{
    public DossierActionValidator()
    {
        RuleFor(x => x.Action).Must(a => a is "invite-interview" or "shortlist" or "reject" or "open" or "reverse-drop");
        When(x => x.Action == "reject", () => RuleFor(x => x.RejectionReasonCode).NotEmpty());
    }
}

public sealed class DossierActionHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<DossierActionCmd, Unit>
{
    public async Task<Unit> Handle(DossierActionCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var app = await db.Applications.FirstOrDefaultAsync(a => a.Id == r.ApplicationId, ct)
            ?? throw new Domain.Common.NotFoundException("Application not found.");

        var isRecruiter = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == app.CompanyId && m.UserId == uid, ct);
        if (!isRecruiter) throw new UnauthorizedAccessException("Only assigned recruiters may act on dossiers.");

        var sla = await db.ApplicationSlas.FirstOrDefaultAsync(s => s.ApplicationId == app.Id, ct);

        switch (r.Action)
        {
            case "open":
                if (app.Status == ApplicationStatus.Delivered)
                {
                    app.Status = ApplicationStatus.Opened;
                    app.OpenedAtUtc = clock.UtcNow;
                }
                break;

            case "shortlist":
                app.Status = ApplicationStatus.Shortlisted;
                if (sla is { Status: SlaStatus.Pending }) { sla.Status = SlaStatus.Responded; sla.RespondedAtUtc = clock.UtcNow; }
                break;

            case "invite-interview":
                app.Status = ApplicationStatus.InterviewScheduled;
                if (sla is { Status: SlaStatus.Pending }) { sla.Status = SlaStatus.Responded; sla.RespondedAtUtc = clock.UtcNow; }
                break;

            case "reject":
                app.Status = ApplicationStatus.Rejected;
                app.RejectionReasonCode = r.RejectionReasonCode;
                app.RejectionNote = r.Note;
                if (sla is { Status: SlaStatus.Pending }) { sla.Status = SlaStatus.Responded; sla.RespondedAtUtc = clock.UtcNow; }

                // Immediate refund: 1 Bonus credit
                var refundKey = $"refund:reject:{app.Id}";
                if (!await db.DropLedgerEntries.AnyAsync(l => l.IdempotencyKey == refundKey, ct))
                {
                    var wallet = await db.DropWalletBalances.FirstOrDefaultAsync(w => w.CandidateProfileId == app.CandidateProfileId, ct);
                    if (wallet == null)
                    {
                        wallet = new DropWalletBalance { CandidateProfileId = app.CandidateProfileId, DailyLimit = 3, DailyRemaining = 3, DailyResetAtUtc = clock.UtcNow.Date.AddDays(1) };
                        db.DropWalletBalances.Add(wallet);
                    }
                    wallet.CreditBonus(1);
                    db.DropLedgerEntries.Add(new DropLedgerEntry
                    {
                        CandidateProfileId = app.CandidateProfileId,
                        Bucket = DropBucket.Bonus.ToString(),
                        Delta = 1,
                        ReasonCode = "RecruiterRejection",
                        ReferenceId = app.Id,
                        IdempotencyKey = refundKey
                    });
                }
                break;

            case "reverse-drop":
                // Deferred to ReverseDrop endpoint
                throw new Domain.Common.DomainRuleException("Use POST /api/v1/reverse-drops.");
        }

        db.ApplicationEvents.Add(new ApplicationEvent
        {
            ApplicationId = app.Id, EventType = r.Action, ActorUserId = uid, ActorType = "User", OccurredAtUtc = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
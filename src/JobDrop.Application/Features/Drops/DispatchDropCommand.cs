using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Entities.Drops;
using ApplicationEntity = JobDrop.Domain.Entities.Drops.Application;
using JobDrop.Domain.Entities.Jobs;
using JobDrop.Domain.Enums;
using JobDrop.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Drops;

public sealed record DispatchDropCommand(Guid JobId, string IdempotencyKey, Guid? ResumeDocumentId, string? CoverNote)
    : IRequest<DispatchDropResult>;

public sealed record DispatchDropResult(
    Guid ApplicationId, string Status, DateTime? DueAtUtc,
    int NewDailyRemaining, int BatchRank, bool IsIdempotentReplay);

public sealed class DispatchDropValidator : AbstractValidator<DispatchDropCommand>
{
    public DispatchDropValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100).Matches("^[a-zA-Z0-9_-]+$");
        RuleFor(x => x.CoverNote).MaximumLength(2000);
    }
}

public sealed class DispatchDropHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<DispatchDropCommand, DispatchDropResult>
{
    public async Task<DispatchDropResult> Handle(DispatchDropCommand r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();

        // Fast-path idempotency: return existing dispatch
        var existing = await db.Applications
            .Where(a => a.IdempotencyKey == r.IdempotencyKey)
            .Select(a => new { a.Id, a.Status, a.BatchRank })
            .FirstOrDefaultAsync(ct);
        if (existing != null)
        {
            var existingSla = await db.ApplicationSlas.AsNoTracking()
                .Where(s => s.ApplicationId == existing.Id).Select(s => s.DueAtUtc).FirstOrDefaultAsync(ct);
            return new DispatchDropResult(existing.Id, existing.Status.ToString(),
                existingSla == default ? null : existingSla, 0, existing.BatchRank ?? 0, true);
        }

        var profile = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (profile == Guid.Empty) throw new Domain.Common.NotFoundException("Candidate profile missing.");

        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == r.JobId, ct)
            ?? throw new Domain.Common.NotFoundException("Job not found.");
        if (job.Status != JobStatus.Published)
            throw new Domain.Common.DomainRuleException("Job is not accepting drops.");

        using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            // Wallet (create if missing)
            var wallet = await db.DropWalletBalances.FirstOrDefaultAsync(w => w.CandidateProfileId == profile, ct);
            if (wallet == null)
            {
                wallet = new DropWalletBalance
                {
                    CandidateProfileId = profile,
                    DailyLimit = 3,
                    DailyRemaining = 3,
                    DailyResetAtUtc = clock.UtcNow.Date.AddDays(1)
                };
                db.DropWalletBalances.Add(wallet);
            }

            var bucket = wallet.TryDebitOne(clock.UtcNow)
                ?? throw new Domain.Common.DomainRuleException("Insufficient drop credits.");

            db.DropLedgerEntries.Add(new DropLedgerEntry
            {
                CandidateProfileId = profile,
                Bucket = bucket.ToString(),
                Delta = -1,
                ReasonCode = "DropDispatch",
                IdempotencyKey = $"dispatch:{r.IdempotencyKey}",
                ReferenceId = null,
                CreatedAtUtc = clock.UtcNow
            });

            // Per-job daily intake
            var marketDate = DateOnly.FromDateTime(clock.UtcNow);
            var intake = await db.JobDailyIntakes
                .FirstOrDefaultAsync(i => i.JobId == job.Id && i.MarketDate == marketDate, ct);
            if (intake == null)
            {
                intake = new JobDailyIntake { JobId = job.Id, MarketDate = marketDate, CapLimit = job.DailyIntakeCap };
                db.JobDailyIntakes.Add(intake);
            }
            if (intake.DeliveredCount >= intake.CapLimit)
                throw new Domain.Common.DomainRuleException("Job daily intake cap reached.");
            intake.DeliveredCount++;

            // Application + Snapshot + SLA
            var now = clock.UtcNow;
            var app = new ApplicationEntity
            {
                JobId = job.Id,
                CompanyId = job.CompanyId,
                CandidateProfileId = profile,
                Status = ApplicationStatus.Delivered,
                DeliveredAtUtc = now,
                BatchRank = intake.DeliveredCount,
                IdempotencyKey = r.IdempotencyKey
            };
            db.Applications.Add(app);

            db.ApplicationSnapshots.Add(new ApplicationSnapshot
            {
                ApplicationId = app.Id,
                ResumeDocumentId = r.ResumeDocumentId,
                ResumeVersion = 1,
                ProfileSnapshotJson = "{}",
                MatchBreakdownJson = "{}",
                MatchScore = 0m,
                ConsentVersion = "v1",
                SharingProtocolJson = "{}",
                CreatedAtUtc = now
            });

            var sla = new ApplicationSla
            {
                ApplicationId = app.Id,
                StartedAtUtc = now,
                DueAtUtc = SlaDeadlineCalculator.ComputeDueAtUtc(now),
                Status = SlaStatus.Pending
            };
            db.ApplicationSlas.Add(sla);

            db.ApplicationEvents.Add(new ApplicationEvent
            {
                ApplicationId = app.Id,
                EventType = "Delivered",
                ActorUserId = uid,
                ActorType = "User",
                OccurredAtUtc = now
            });

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new DispatchDropResult(app.Id, "Delivered", sla.DueAtUtc, wallet.DailyRemaining, app.BatchRank ?? 0, false);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
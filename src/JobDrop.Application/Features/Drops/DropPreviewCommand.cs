using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Drops;

public sealed record DropPreviewQuery(Guid JobId) : IRequest<DropPreviewResult>;
public sealed record DropPreviewResult(
    bool Eligible, string[] Reasons, int WalletCost, string BucketUsed,
    int RemainingIntakeSlots, DateTime? SlaPromiseDueAtUtc);

public sealed class DropPreviewHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<DropPreviewQuery, DropPreviewResult>
{
    public async Task<DropPreviewResult> Handle(DropPreviewQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var reasons = new List<string>();

        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (profileId == Guid.Empty) reasons.Add("NO_PROFILE");

        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == r.JobId, ct);
        if (job == null) reasons.Add("JOB_NOT_FOUND");
        else if (job.Status != JobStatus.Published) reasons.Add("JOB_NOT_PUBLISHED");

        var already = await db.Applications.AsNoTracking()
            .AnyAsync(a => a.JobId == r.JobId && a.CandidateProfileId == profileId, ct);
        if (already) reasons.Add("ALREADY_DROPPED");

        var wallet = await db.DropWalletBalances.AsNoTracking()
            .FirstOrDefaultAsync(w => w.CandidateProfileId == profileId, ct);
        var remainingDaily = wallet == null
            ? 3
            : (clock.UtcNow >= wallet.DailyResetAtUtc ? wallet.DailyLimit : wallet.DailyRemaining);
        var totalCredits = remainingDaily + (wallet?.BonusBalance ?? 0) + (wallet?.SpecialBalance ?? 0);
        if (totalCredits <= 0) reasons.Add("NO_CREDITS");

        var marketDate = DateOnly.FromDateTime(clock.UtcNow);
        var intake = job == null ? null : await db.JobDailyIntakes.AsNoTracking()
            .FirstOrDefaultAsync(i => i.JobId == job.Id && i.MarketDate == marketDate, ct);
        var used = intake?.DeliveredCount ?? 0;
        var cap = job?.DailyIntakeCap ?? 0;
        if (job != null && used >= cap) reasons.Add("JOB_INTAKE_FULL");

        var bucket = remainingDaily > 0 ? "Daily"
                   : (wallet?.BonusBalance ?? 0) > 0 ? "Bonus"
                   : "Special";

        return new DropPreviewResult(
            Eligible: reasons.Count == 0,
            Reasons: reasons.ToArray(),
            WalletCost: 1,
            BucketUsed: bucket,
            RemainingIntakeSlots: Math.Max(0, cap - used),
            SlaPromiseDueAtUtc: clock.UtcNow.AddHours(48));
    }
}
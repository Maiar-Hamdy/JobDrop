using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Drops;

public sealed record GetWalletQuery : IRequest<WalletDto>;
public sealed record GetLedgerQuery(int Take) : IRequest<IReadOnlyList<LedgerDto>>;

public sealed class GetWalletHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<GetWalletQuery, WalletDto>
{
    public async Task<WalletDto> Handle(GetWalletQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);

        var wallet = await db.DropWalletBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CandidateProfileId == profileId, ct);

        if (wallet == null)
            return new WalletDto(3, 3, clock.UtcNow.Date.AddDays(1), 0, 0);

        // Apply lazy reset for display only
        if (clock.UtcNow >= wallet.DailyResetAtUtc)
            return new WalletDto(wallet.DailyLimit, wallet.DailyLimit, clock.UtcNow.Date.AddDays(1), wallet.BonusBalance, wallet.SpecialBalance);

        return new WalletDto(wallet.DailyRemaining, wallet.DailyLimit, wallet.DailyResetAtUtc, wallet.BonusBalance, wallet.SpecialBalance);
    }
}

public sealed class GetLedgerHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetLedgerQuery, IReadOnlyList<LedgerDto>>
{
    public async Task<IReadOnlyList<LedgerDto>> Handle(GetLedgerQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        return await db.DropLedgerEntries.AsNoTracking()
            .Where(l => l.CandidateProfileId == profileId)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(Math.Clamp(r.Take, 1, 200))
            .Select(l => new LedgerDto(l.Id, l.Bucket, l.Delta, l.ReasonCode, l.ReferenceId, l.CreatedAtUtc))
            .ToListAsync(ct);
    }
}
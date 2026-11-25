using JobDrop.Application.Contracts.Billing;
using JobDrop.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Infrastructure.Billing;

public sealed class EntitlementResolver(IApplicationDbContext db) : IEntitlementResolver
{
    public async Task<int> GetDailyDropAllowanceAsync(Guid candidateProfileId, CancellationToken ct)
    {
        var wallet = await db.DropWalletBalances.AsNoTracking()
            .FirstOrDefaultAsync(w => w.CandidateProfileId == candidateProfileId, ct);
        return wallet?.DailyLimit ?? 3;
    }

    public async Task<int> GetMonthlyReverseDropAllowanceAsync(Guid companyId, CancellationToken ct)
    {
        var pool = await db.ReverseDropPools.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CompanyId == companyId, ct);
        return pool == null ? 0 : pool.MonthlyRemaining;
    }

    public Task<bool> HasProFeatureAsync(Guid userId, string featureKey, CancellationToken ct)
        => Task.FromResult(false); // Free tier default
}
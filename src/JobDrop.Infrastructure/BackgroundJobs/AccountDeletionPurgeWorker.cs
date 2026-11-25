using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class AccountDeletionPurgeWorker(IServiceProvider sp, ILogger<AccountDeletionPurgeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var now = DateTime.UtcNow;

                // Process pending requests older than 30-day grace period
                var eligible = await db.Set<AccountDeletionRequest>()
                    .Where(r => r.Status == "Pending" && r.RequestedAtUtc < now.AddDays(-30))
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var req in eligible)
                {
                    req.Status = "Purging";
                    await db.SaveChangesAsync(stoppingToken);

                    // Purge PII (candidate profile summary, starting points)
                    var profile = await db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == req.UserId, stoppingToken);
                    if (profile != null)
                    {
                        profile.Headline = null;
                        profile.Summary = null;
                        profile.SharingProtocolJson = "{}";
                    }

                    // Retain anonymized ledger + audit logs (per compliance)
                    req.Status = "Completed";
                    req.CompletedAtUtc = now;
                    await db.SaveChangesAsync(stoppingToken);
                    logger.LogInformation("Account {UserId} purged (anonymized)", req.UserId);
                }
            }
            catch (Exception ex) { logger.LogError(ex, "AccountDeletionPurgeWorker failed"); }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
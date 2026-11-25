using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Entities.Drops;
using JobDrop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class SlaExpirationRefundWorker(IServiceProvider sp, ILogger<SlaExpirationRefundWorker> logger) : BackgroundService
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

                var expired = await db.ApplicationSlas
                    .Where(s => s.Status == SlaStatus.Pending && s.DueAtUtc <= now)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var sla in expired)
                {
                    var app = await db.Applications.FirstOrDefaultAsync(a => a.Id == sla.ApplicationId, stoppingToken);
                    if (app == null) continue;

                    var refundKey = $"refund:sla:{app.Id}";
                    if (await db.DropLedgerEntries.AnyAsync(l => l.IdempotencyKey == refundKey, stoppingToken))
                        continue;

                    sla.Status = SlaStatus.ExpiredRefunded;
                    sla.RespondedAtUtc = now;
                    app.Status = ApplicationStatus.ExpiredRefunded;

                    var wallet = await db.DropWalletBalances.FirstOrDefaultAsync(w => w.CandidateProfileId == app.CandidateProfileId, stoppingToken);
                    if (wallet == null)
                    {
                        wallet = new DropWalletBalance
                        {
                            CandidateProfileId = app.CandidateProfileId,
                            DailyLimit = 3, DailyRemaining = 3,
                            DailyResetAtUtc = now.Date.AddDays(1)
                        };
                        db.DropWalletBalances.Add(wallet);
                    }
                    wallet.CreditBonus(1);

                    db.DropLedgerEntries.Add(new DropLedgerEntry
                    {
                        CandidateProfileId = app.CandidateProfileId,
                        Bucket = DropBucket.Bonus.ToString(),
                        Delta = 1,
                        ReasonCode = "SlaExpiredRefund",
                        ReferenceId = app.Id,
                        IdempotencyKey = refundKey,
                        CreatedAtUtc = now
                    });

                    db.ApplicationEvents.Add(new ApplicationEvent
                    {
                        ApplicationId = app.Id, EventType = "ExpiredRefunded",
                        ActorType = "System", OccurredAtUtc = now
                    });

                    logger.LogInformation("SLA expired for {AppId}; refunded 1 Bonus", app.Id);
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { logger.LogError(ex, "SlaExpirationRefundWorker cycle failed"); }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
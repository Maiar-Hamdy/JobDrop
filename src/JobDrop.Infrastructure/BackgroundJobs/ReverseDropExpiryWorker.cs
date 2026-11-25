using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class ReverseDropExpiryWorker(IServiceProvider sp, ILogger<ReverseDropExpiryWorker> logger) : BackgroundService
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
                var expired = await db.ReverseDrops
                    .Where(r => r.Status == ReverseDropStatus.Sent && r.ExpiresAtUtc <= now)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var rd in expired)
                {
                    rd.Status = ReverseDropStatus.ExpiredRefunded;
                    var pool = await db.ReverseDropPools.FirstOrDefaultAsync(p => p.CompanyId == rd.CompanyId, stoppingToken);
                    if (pool != null) pool.TopUpRemaining++;
                    logger.LogInformation("ReverseDrop {Id} expired; refunded 1 credit", rd.Id);
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { logger.LogError(ex, "ReverseDropExpiryWorker failed"); }
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
using System.Text;
using JobDrop.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class ExportJobWorker(IServiceProvider sp, ILogger<ExportJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var queued = await db.ExportJobs
                    .Where(j => j.Status == "Queued")
                    .Take(5)
                    .ToListAsync(stoppingToken);

                foreach (var job in queued)
                {
                    job.Status = "Processing";
                    await db.SaveChangesAsync(stoppingToken);

                    var sb = new StringBuilder();
                    sb.AppendLine("ApplicationId,Status,DeliveredAtUtc");
                    var profileId = await db.CandidateProfiles.AsNoTracking()
                        .Where(p => p.UserId == job.UserId).Select(p => p.Id).FirstOrDefaultAsync(stoppingToken);
                    var apps = await db.Applications.AsNoTracking()
                        .Where(a => a.CandidateProfileId == profileId)
                        .Select(a => new { a.Id, a.Status, a.DeliveredAtUtc })
                        .ToListAsync(stoppingToken);
                    foreach (var a in apps)
                        sb.AppendLine($"{a.Id},{a.Status},{a.DeliveredAtUtc:O}");

                    job.StorageKey = $"exports/{job.Id}.csv";
                    job.Status = "Completed";
                    job.DownloadUrlExpiresAtUtc = DateTime.UtcNow.AddHours(1);
                    await db.SaveChangesAsync(stoppingToken);
                    logger.LogInformation("Export {Id} generated ({Bytes} bytes)", job.Id, sb.Length);
                }
            }
            catch (Exception ex) { logger.LogError(ex, "ExportJobWorker cycle failed"); }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
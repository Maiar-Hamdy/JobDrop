using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class DocumentScanWorker(IServiceProvider sp, ILogger<DocumentScanWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var pending = await db.CandidateDocuments
                    .Where(d => d.ScanStatus == DocumentScanStatus.PendingScan)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var d in pending)
                {
                    // Simulated magic-byte + malware scan (real impl would call external scanner)
                    if (!string.IsNullOrEmpty(d.Sha256Hash) && d.Sha256Hash.Length == 64)
                    {
                        d.ScanStatus = DocumentScanStatus.Clean;
                        d.RedactedStorageKey = d.StorageKey.Replace("/original/", "/redacted/");
                        logger.LogInformation("Document {Id} scanned clean; redacted key generated", d.Id);
                    }
                    else
                    {
                        d.ScanStatus = DocumentScanStatus.Quarantined;
                        logger.LogWarning("Document {Id} quarantined (invalid hash)", d.Id);
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { logger.LogError(ex, "DocumentScanWorker cycle failed"); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
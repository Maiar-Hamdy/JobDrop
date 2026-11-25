using System.Text.Json;
using JobDrop.Application.Contracts.Notifications;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.BackgroundJobs;

public sealed class OutboxNotificationWorker(IServiceProvider sp, ILogger<OutboxNotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var wa = scope.ServiceProvider.GetRequiredService<IWhatsAppSender>();
                var push = scope.ServiceProvider.GetRequiredService<IPushSender>();

                var now = DateTime.UtcNow;
                var batch = await db.OutboxMessages
                    .Where(m => m.DeliveredAtUtc == null && m.DeadLetteredAtUtc == null && m.NextAttemptAtUtc <= now)
                    .Take(30)
                    .ToListAsync(stoppingToken);

                foreach (var msg in batch)
                {
                    try
                    {
                        switch (msg.Channel)
                        {
                            case "Email":
                                if (!string.IsNullOrEmpty(msg.RecipientAddress))
                                    await email.SendAsync(msg.RecipientAddress, msg.EventType, msg.PayloadJson, stoppingToken);
                                break;
                            case "WhatsApp":
                                if (!string.IsNullOrEmpty(msg.RecipientAddress))
                                    await wa.SendAsync(msg.RecipientAddress, msg.EventType, JsonSerializer.Deserialize<object>(msg.PayloadJson) ?? new { }, stoppingToken);
                                break;
                            case "Push":
                                if (msg.RecipientUserId.HasValue)
                                    await push.SendAsync(msg.RecipientUserId.Value, msg.EventType, msg.PayloadJson, stoppingToken);
                                break;
                        }
                        msg.DeliveredAtUtc = now;
                    }
                    catch (Exception ex)
                    {
                        msg.RetryCount++;
                        msg.LastError = ex.Message;
                        if (msg.RetryCount >= 5)
                        {
                            msg.DeadLetteredAtUtc = now;
                            logger.LogError(ex, "Outbox message {Id} dead-lettered after 5 retries", msg.Id);
                        }
                        else
                        {
                            msg.NextAttemptAtUtc = now.AddSeconds(Math.Pow(2, msg.RetryCount) * 5);
                            logger.LogWarning(ex, "Outbox message {Id} retry {Retry}", msg.Id, msg.RetryCount);
                        }
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { logger.LogError(ex, "OutboxNotificationWorker cycle failed"); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
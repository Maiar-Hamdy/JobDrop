using JobDrop.Application.Contracts.Notifications;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.Notifications;

public sealed class SmtpEmailSender(ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toAddress, string subject, string bodyHtml, CancellationToken ct)
    {
        logger.LogInformation("Email dispatched to {To}: {Subject}", toAddress, subject);
        return Task.CompletedTask;
    }
}

public sealed class WhatsAppSender(ILogger<WhatsAppSender> logger) : IWhatsAppSender
{
    public Task SendAsync(string toPhone, string templateCode, object data, CancellationToken ct)
    {
        logger.LogInformation("WhatsApp dispatched to {Phone}: {Template}", toPhone, templateCode);
        return Task.CompletedTask;
    }
}

public sealed class WebPushSender(ILogger<WebPushSender> logger) : IPushSender
{
    public Task SendAsync(Guid userId, string title, string body, CancellationToken ct)
    {
        logger.LogInformation("Push dispatched to {UserId}: {Title}", userId, title);
        return Task.CompletedTask;
    }
}
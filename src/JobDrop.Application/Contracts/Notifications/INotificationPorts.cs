namespace JobDrop.Application.Contracts.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string bodyHtml, CancellationToken ct);
}

public interface IWhatsAppSender
{
    Task SendAsync(string toPhone, string templateCode, object data, CancellationToken ct);
}

public interface IPushSender
{
    Task SendAsync(Guid userId, string title, string body, CancellationToken ct);
}
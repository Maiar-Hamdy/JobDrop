using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Notifications;
public sealed class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = default!;
    public string Channel { get; set; } = "InApp";                 // InApp | Email | WhatsApp | Push
    public Guid? RecipientUserId { get; set; }
    public string? RecipientAddress { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public int RetryCount { get; set; }
    public DateTime NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? DeadLetteredAtUtc { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Billing;
public sealed class PaymentWebhookEvent : BaseEntity
{
    public string Provider { get; set; } = "paymob";
    public string ProviderTransactionId { get; set; } = default!;
    public string EventStateKey { get; set; } = default!;         // Pending | Success | Refunded | Voided | Failed
    public string PayloadHash { get; set; } = default!;
    public string ProcessingStatus { get; set; } = "Processed";   // Processed | Skipped | Failed
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
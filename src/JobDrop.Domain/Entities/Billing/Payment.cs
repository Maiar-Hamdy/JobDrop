using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Billing;
public sealed class Payment : BaseEntity
{
    public Guid? SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public string MerchantReference { get; set; } = default!;    // our idempotency ref
    public string ProviderOrderId { get; set; } = default!;      // Paymob order.id
    public string? ProviderIntentionId { get; set; }
    public string? ProviderTransactionId { get; set; }
    public long AmountCents { get; set; }
    public string Currency { get; set; } = "AED";
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public string? FailureReason { get; set; }
    public byte[]? RowVersion { get; set; }
}
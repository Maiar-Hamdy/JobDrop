using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Billing;
public sealed class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid PlanId { get; set; }
    public string Status { get; set; } = "Active";        // Active | Cancelled | PastDue | Expired
    public DateTime StartedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }
}
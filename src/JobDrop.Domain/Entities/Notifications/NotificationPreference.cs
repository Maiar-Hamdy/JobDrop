using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Notifications;
public sealed class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public string Category { get; set; } = default!;               // DropDelivered | SlaWarning | Interview | Billing
    public bool InApp { get; set; } = true;
    public bool Email { get; set; } = true;
    public bool WhatsApp { get; set; } = false;
    public bool Push { get; set; } = true;
}
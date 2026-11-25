using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Identity;
public sealed class AccountDeletionRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
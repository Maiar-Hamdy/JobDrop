using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Identity;

public sealed class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = default!;
    public Guid FamilyId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;
}
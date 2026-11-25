using Microsoft.AspNetCore.Identity;
namespace JobDrop.Infrastructure.Persistence;

public sealed class AppIdentityUser : IdentityUser<Guid>
{
    public string Handle { get; set; } = default!;
    public string? AvatarCode { get; set; }
    public string? GoogleSubjectId { get; set; }
    public int TokenVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Candidate;
public sealed class AuditLog : BaseEntity
{
    public Guid? ActorUserId { get; set; }
    public string ActorType { get; set; } = "User";              // User | System | Admin
    public Guid? CompanyId { get; set; }
    public string Action { get; set; } = default!;
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
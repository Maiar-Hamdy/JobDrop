using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Reviews;
public sealed class Review : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public string TargetType { get; set; } = default!;            // Company | Candidate
    public Guid TargetId { get; set; }
    public int Rating { get; set; }                                // 1..5
    public string? Comment { get; set; }
    public string Status { get; set; } = "Pending";                // Pending | Approved | Rejected | Hidden
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
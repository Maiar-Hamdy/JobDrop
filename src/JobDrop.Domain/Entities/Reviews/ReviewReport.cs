using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Reviews;
public sealed class ReviewReport : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Guid ReporterUserId { get; set; }
    public string Reason { get; set; } = default!;
    public string Status { get; set; } = "Open";                   // Open | Resolved | Dismissed
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
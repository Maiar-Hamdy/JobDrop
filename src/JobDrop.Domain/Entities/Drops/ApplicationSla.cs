using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Drops;
public sealed class ApplicationSla : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }
    public SlaStatus Status { get; set; } = SlaStatus.Pending;
    public string PolicyVersion { get; set; } = "v1";
    public string CalendarSnapshotJson { get; set; } = "{}";
    public byte[]? RowVersion { get; set; }
}
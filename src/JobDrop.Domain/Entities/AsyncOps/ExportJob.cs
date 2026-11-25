using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.AsyncOps;
public sealed class ExportJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string Kind { get; set; } = default!;                   // DropsCsv | DropsXlsx | PersonalInsightsXlsx | RequisitionsCsv
    public string Status { get; set; } = "Queued";                 // Queued | Processing | Completed | Failed
    public string? StorageKey { get; set; }
    public DateTime? DownloadUrlExpiresAtUtc { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Drops;
public sealed class ApplicationSnapshot : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public Guid? ResumeDocumentId { get; set; }
    public int ResumeVersion { get; set; }
    public string? RedactedResumeKey { get; set; }
    public string ProfileSnapshotJson { get; set; } = "{}";
    public string MatchBreakdownJson { get; set; } = "{}";
    public decimal MatchScore { get; set; }
    public string ConsentVersion { get; set; } = "v1";
    public string SharingProtocolJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
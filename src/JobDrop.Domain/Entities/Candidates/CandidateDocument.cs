using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class CandidateDocument : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string Kind { get; set; } = "CV";
    public string OriginalFileName { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
    public string? RedactedStorageKey { get; set; }
    public string Sha256Hash { get; set; } = default!;
    public long SizeBytes { get; set; }
    public DocumentScanStatus ScanStatus { get; set; } = DocumentScanStatus.PendingScan;
    public int Version { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
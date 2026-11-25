using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Drops;
public sealed class Application : AggregateRoot
{
    public Guid JobId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Queued;
    public int? BatchRank { get; set; }
    public string? RejectionReasonCode { get; set; }
    public string? RejectionNote { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? OpenedAtUtc { get; set; }
    public string? IdempotencyKey { get; set; }
    public byte[]? RowVersion { get; set; }
}
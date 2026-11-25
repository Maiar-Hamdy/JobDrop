using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class DropLedgerEntry : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string Bucket { get; set; } = default!;   // Daily|Bonus|Special
    public int Delta { get; set; }
    public string ReasonCode { get; set; } = default!;
    public Guid? ReferenceId { get; set; }
    public string IdempotencyKey { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
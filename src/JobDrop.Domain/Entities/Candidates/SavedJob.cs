using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class SavedJob : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public Guid JobId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class PinnedDrop : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public Guid ApplicationId { get; set; }
    public int PinOrder { get; set; } // 1..3
}
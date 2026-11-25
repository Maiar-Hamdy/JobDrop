using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Messaging;
public sealed class Conversation : BaseEntity
{
    public Guid? ApplicationId { get; set; }
    public Guid? ReverseDropId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public bool IsArchivedByCandidate { get; set; }
    public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;
}
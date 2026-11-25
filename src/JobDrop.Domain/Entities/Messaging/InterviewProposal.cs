using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Messaging;
public sealed class InterviewProposal : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid ProposedByUserId { get; set; }
    public string CallType { get; set; } = "Video"; // Voice|Video
    public string SlotsJson { get; set; } = "[]";
    public DateTime? ConfirmedStartUtc { get; set; }
    public DateTime? ConfirmedEndUtc { get; set; }
    public string? MeetingUrl { get; set; }
    public string Status { get; set; } = "Proposed"; // Proposed|Confirmed|Cancelled
}
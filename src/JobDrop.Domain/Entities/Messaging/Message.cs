using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Messaging;
public sealed class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Body { get; set; } = default!;
    public string? AttachmentKey { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
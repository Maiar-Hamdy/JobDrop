using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Drops;
public sealed class ApplicationEvent : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public string EventType { get; set; } = default!;
    public Guid? ActorUserId { get; set; }
    public string ActorType { get; set; } = "User";
    public string? DataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
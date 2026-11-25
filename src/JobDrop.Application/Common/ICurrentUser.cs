namespace JobDrop.Application.Common;
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    int TokenVersion { get; }
    string? CorrelationId { get; }
}
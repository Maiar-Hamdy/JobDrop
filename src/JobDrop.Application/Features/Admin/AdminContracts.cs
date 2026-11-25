using MediatR;
namespace JobDrop.Application.Features.Admin;

public sealed record AdminDashboardDto(
    int Users, int Candidates, int Companies,
    int PublishedJobs, int PausedJobs, int ClosedJobs,
    int DeliveredDrops, int QueuedDrops, int RefundedDrops,
    int ActiveSubscriptions,
    int PendingVerifications,
    int PendingReviews,
    int OutboxBacklog, int OutboxDeadLettered);

public sealed record GetAdminDashboardQuery : IRequest<AdminDashboardDto>;

public sealed record VerificationDecisionCmd(Guid RequestId, string Decision, string? Note) : IRequest<Unit>;
public sealed record ModerationDecisionCmd(Guid ReviewId, string Decision) : IRequest<Unit>;
public sealed record ReplayDlqCmd : IRequest<int>;
public sealed record ListAuditLogsQuery(int Page, int PageSize) : IRequest<object>;
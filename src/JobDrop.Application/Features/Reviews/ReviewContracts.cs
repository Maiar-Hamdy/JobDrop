using MediatR;
namespace JobDrop.Application.Features.Reviews;

public sealed record SubmitReviewCmd(Guid ApplicationId, int Rating, string? Comment, string TargetType) : IRequest<Guid>;
public sealed record ReportReviewCmd(Guid ReviewId, string Reason) : IRequest<Guid>;
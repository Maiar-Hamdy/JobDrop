using MediatR;
namespace JobDrop.Application.Features.Jobs;

public sealed record JobDto(
    Guid Id, Guid CompanyId, Guid OfficeId, string Title, string WorkMode,
    decimal? SalaryMin, decimal? SalaryMax, string? SalaryCurrency,
    int DailyIntakeCap, string Status, DateTime? PublishedAtUtc);

public sealed record CreateJobCmd(
    Guid CompanyId, Guid OfficeId, string Title, string WorkMode,
    decimal? SalaryMin, decimal? SalaryMax, string? SalaryCurrency,
    int DailyIntakeCap) : MediatR.IRequest<Guid>;

public sealed record JobLifecycleCmd(Guid JobId, string Action) : MediatR.IRequest<Unit>;
public sealed record GetJobQuery(Guid JobId) : MediatR.IRequest<JobDto>;
public sealed record UpdateIntakePolicyCmd(Guid JobId, int NewDailyIntakeCap) : MediatR.IRequest<Unit>;
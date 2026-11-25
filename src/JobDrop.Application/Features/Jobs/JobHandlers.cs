using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Jobs;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Jobs;

public sealed class CreateJobValidator : AbstractValidator<CreateJobCmd>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkMode).Must(v => v is "OnSite" or "Hybrid" or "Remote");
        RuleFor(x => x.DailyIntakeCap).InclusiveBetween(1, 200);
        RuleFor(x => x.SalaryMin).Must((c, m) => !m.HasValue || !c.SalaryMax.HasValue || m <= c.SalaryMax);
    }
}

public sealed class CreateJobHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<CreateJobCmd, Guid>
{
    public async Task<Guid> Handle(CreateJobCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var isMember = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == r.CompanyId && m.UserId == uid, ct);
        if (!isMember) throw new Domain.Common.DomainRuleException("Not a member of the company.");

        var job = new Job
        {
            CompanyId = r.CompanyId,
            OfficeId = r.OfficeId,
            OwnerUserId = uid,
            Title = r.Title,
            WorkMode = Enum.Parse<WorkMode>(r.WorkMode),
            SalaryMin = r.SalaryMin,
            SalaryMax = r.SalaryMax,
            SalaryCurrency = r.SalaryCurrency,
            DailyIntakeCap = r.DailyIntakeCap,
            Status = JobStatus.Draft
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);
        return job.Id;
    }
}

public sealed class JobLifecycleValidator : AbstractValidator<JobLifecycleCmd>
{
    public JobLifecycleValidator()
        => RuleFor(x => x.Action).Must(a => a is "publish" or "pause-intake" or "resume-intake" or "close" or "reopen");
}

public sealed class JobLifecycleHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<JobLifecycleCmd, Unit>
{
    public async Task<Unit> Handle(JobLifecycleCmd r, CancellationToken ct)
    {
        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == r.JobId, ct)
            ?? throw new Domain.Common.NotFoundException("Job not found.");
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var isMember = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == job.CompanyId && m.UserId == uid, ct);
        if (!isMember) throw new Domain.Common.DomainRuleException("Not a member of the company.");

        switch (r.Action)
        {
            case "publish":
                if (job.Status != JobStatus.Draft && job.Status != JobStatus.Closed) throw new Domain.Common.DomainRuleException("Cannot publish.");
                job.Status = JobStatus.Published;
                job.PublishedAtUtc = clock.UtcNow;
                break;
            case "pause-intake": job.Status = JobStatus.Paused; break;
            case "resume-intake":
                if (job.Status != JobStatus.Paused) throw new Domain.Common.DomainRuleException("Not paused.");
                job.Status = JobStatus.Published; break;
            case "close": job.Status = JobStatus.Closed; break;
            case "reopen":
                if (job.Status != JobStatus.Closed) throw new Domain.Common.DomainRuleException("Not closed.");
                job.Status = JobStatus.Published;
                job.PublishedAtUtc = clock.UtcNow; break;
        }
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public sealed class UpdateIntakePolicyValidator : AbstractValidator<UpdateIntakePolicyCmd>
{
    public UpdateIntakePolicyValidator() => RuleFor(x => x.NewDailyIntakeCap).InclusiveBetween(1, 200);
}

public sealed class UpdateIntakePolicyHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateIntakePolicyCmd, Unit>
{
    public async Task<Unit> Handle(UpdateIntakePolicyCmd r, CancellationToken ct)
    {
        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == r.JobId, ct)
            ?? throw new Domain.Common.NotFoundException("Job not found.");
        job.DailyIntakeCap = r.NewDailyIntakeCap;
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public sealed class GetJobHandler(IApplicationDbContext db)
    : IRequestHandler<GetJobQuery, JobDto>
{
    public async Task<JobDto> Handle(GetJobQuery r, CancellationToken ct)
        => await db.Jobs.AsNoTracking()
            .Where(j => j.Id == r.JobId)
            .Select(j => new JobDto(j.Id, j.CompanyId, j.OfficeId, j.Title, j.WorkMode.ToString(),
                j.SalaryMin, j.SalaryMax, j.SalaryCurrency, j.DailyIntakeCap, j.Status.ToString(), j.PublishedAtUtc))
            .FirstOrDefaultAsync(ct) ?? throw new Domain.Common.NotFoundException("Job not found.");
}
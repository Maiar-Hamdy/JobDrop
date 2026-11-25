using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.AsyncOps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Exports;

public sealed class RequestExportValidator : AbstractValidator<RequestExportCmd>
{
    public RequestExportValidator()
        => RuleFor(x => x.Kind).Must(k => k is "DropsCsv" or "DropsXlsx" or "PersonalInsightsXlsx" or "RequisitionsCsv");
}

public sealed class RequestExportHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<RequestExportCmd, Guid>
{
    public async Task<Guid> Handle(RequestExportCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var job = new ExportJob { UserId = uid, Kind = r.Kind, Status = "Queued", CreatedAtUtc = clock.UtcNow };
        db.ExportJobs.Add(job);
        await db.SaveChangesAsync(ct);
        return job.Id;
    }
}

public sealed class GetExportStatusHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetExportStatusQuery, ExportStatusDto>
{
    public async Task<ExportStatusDto> Handle(GetExportStatusQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var job = await db.ExportJobs.AsNoTracking().FirstOrDefaultAsync(e => e.Id == r.Id && e.UserId == uid, ct)
            ?? throw new Domain.Common.NotFoundException("Export not found.");
        return new ExportStatusDto(job.Id, job.Status, job.StorageKey, job.DownloadUrlExpiresAtUtc);
    }
}
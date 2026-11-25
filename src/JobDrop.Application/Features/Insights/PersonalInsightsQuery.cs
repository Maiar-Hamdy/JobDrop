using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Insights;

public sealed record PersonalInsightsDto(
    int SentCount, int LandedCount, int OpenedCount, int ReviewedCount,
    int ShortlistedCount, int InterviewScheduledCount, int OfferExtendedCount,
    int RejectedCount, int RefundedCount);

public sealed record GetPersonalInsightsQuery(string Period) : IRequest<PersonalInsightsDto>;

public sealed class GetPersonalInsightsHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<GetPersonalInsightsQuery, PersonalInsightsDto>
{
    public async Task<PersonalInsightsDto> Handle(GetPersonalInsightsQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);

        var since = r.Period switch
        {
            "day"   => clock.UtcNow.Date,
            "month" => new DateTime(clock.UtcNow.Year, clock.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _       => DateTime.MinValue
        };

        var apps = await db.Applications.AsNoTracking()
            .Where(a => a.CandidateProfileId == profileId && a.DeliveredAtUtc != null && a.DeliveredAtUtc >= since)
            .Select(a => a.Status)
            .ToListAsync(ct);

        return new PersonalInsightsDto(
            SentCount: apps.Count,
            LandedCount: apps.Count(s => s == ApplicationStatus.Delivered),
            OpenedCount: apps.Count(s => s == ApplicationStatus.Opened),
            ReviewedCount: apps.Count(s => s == ApplicationStatus.Reviewed),
            ShortlistedCount: apps.Count(s => s == ApplicationStatus.Shortlisted),
            InterviewScheduledCount: apps.Count(s => s == ApplicationStatus.InterviewScheduled),
            OfferExtendedCount: apps.Count(s => s == ApplicationStatus.OfferExtended),
            RejectedCount: apps.Count(s => s == ApplicationStatus.Rejected),
            RefundedCount: apps.Count(s => s == ApplicationStatus.ExpiredRefunded));
    }
}
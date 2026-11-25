using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Drops;

public sealed record MyDropsSummaryDto(
    int DailyRemaining, int DailyLimit, DateTime DailyResetAtUtc,
    int BonusBalance, int SpecialBalance,
    int ActiveDrops, int RepliesCount, int RefundedCount);

public sealed record MyDropsItemDto(
    Guid ApplicationId, Guid JobId, string JobTitle, string CompanyDisplayName,
    string Status, DateTime? DeliveredAtUtc, DateTime? DueAtUtc, int? BatchRank);

public sealed record GetMyDropsSummaryQuery : IRequest<MyDropsSummaryDto>;
public sealed record GetMyDropsListQuery(DropTab Tab, string? Search, int Page, int PageSize) : IRequest<PagedList<MyDropsItemDto>>;

public sealed class GetMyDropsSummaryHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<GetMyDropsSummaryQuery, MyDropsSummaryDto>
{
    public async Task<MyDropsSummaryDto> Handle(GetMyDropsSummaryQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);

        var wallet = await db.DropWalletBalances.AsNoTracking()
            .FirstOrDefaultAsync(w => w.CandidateProfileId == profileId, ct);
        var dailyRemaining = wallet == null ? 3 : (clock.UtcNow >= wallet.DailyResetAtUtc ? wallet.DailyLimit : wallet.DailyRemaining);

        var now = clock.UtcNow;
        var active = await db.Applications.AsNoTracking()
            .Where(a => a.CandidateProfileId == profileId
                        && (a.Status == ApplicationStatus.Delivered || a.Status == ApplicationStatus.Opened))
            .Join(db.ApplicationSlas, a => a.Id, s => s.ApplicationId, (a, s) => new { a, s })
            .CountAsync(x => x.s.Status == SlaStatus.Pending && x.s.DueAtUtc > now, ct);

        var replies = await db.Applications.AsNoTracking()
            .CountAsync(a => a.CandidateProfileId == profileId
                          && (a.Status == ApplicationStatus.Shortlisted
                           || a.Status == ApplicationStatus.InterviewScheduled
                           || a.Status == ApplicationStatus.OfferExtended), ct);

        var refunded = await db.Applications.AsNoTracking()
            .CountAsync(a => a.CandidateProfileId == profileId && a.Status == ApplicationStatus.ExpiredRefunded, ct);

        return new MyDropsSummaryDto(
            dailyRemaining, wallet?.DailyLimit ?? 3,
            wallet?.DailyResetAtUtc ?? now.Date.AddDays(1),
            wallet?.BonusBalance ?? 0, wallet?.SpecialBalance ?? 0,
            active, replies, refunded);
    }
}

public sealed class GetMyDropsListHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetMyDropsListQuery, PagedList<MyDropsItemDto>>
{
    public async Task<PagedList<MyDropsItemDto>> Handle(GetMyDropsListQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);

        var q = from a in db.Applications.AsNoTracking()
                join j in db.Jobs.AsNoTracking() on a.JobId equals j.Id
                join c in db.Companies.AsNoTracking() on a.CompanyId equals c.Id
                join s in db.ApplicationSlas.AsNoTracking() on a.Id equals s.ApplicationId into sla
                from s2 in sla.DefaultIfEmpty()
                where a.CandidateProfileId == profileId
                select new { a, j, c, s2 };

        if (r.Tab != DropTab.All)
        {
            var statuses = r.Tab switch
            {
                DropTab.Active   => new[] { ApplicationStatus.Delivered, ApplicationStatus.Opened },
                DropTab.Landed   => new[] { ApplicationStatus.Delivered },
                DropTab.Opened   => new[] { ApplicationStatus.Opened, ApplicationStatus.Reviewed },
                DropTab.Shortlisted => new[] { ApplicationStatus.Shortlisted, ApplicationStatus.InterviewScheduled, ApplicationStatus.OfferExtended },
                DropTab.Replied  => new[] { ApplicationStatus.InterviewScheduled, ApplicationStatus.OfferExtended },
                DropTab.ExpiredOrRefunded => new[] { ApplicationStatus.ExpiredRefunded, ApplicationStatus.Withdrawn, ApplicationStatus.Cancelled },
                _ => Array.Empty<ApplicationStatus>()
            };
            q = q.Where(x => statuses.Contains(x.a.Status));
        }

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim();
            q = q.Where(x => EF.Functions.Like(x.j.Title, $"%{s}%") || EF.Functions.Like(x.c.DisplayName, $"%{s}%"));
        }

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, r.Page);
        var size = Math.Clamp(r.PageSize, 1, 100);

        var items = await q
            .OrderByDescending(x => x.a.DeliveredAtUtc)
            .Skip((page - 1) * size).Take(size)
            .Select(x => new MyDropsItemDto(
                x.a.Id, x.j.Id, x.j.Title, x.c.DisplayName,
                x.a.Status.ToString(), x.a.DeliveredAtUtc, x.s2 != null ? x.s2.DueAtUtc : (DateTime?)null,
                x.a.BatchRank))
            .ToListAsync(ct);

        return new PagedList<MyDropsItemDto>(items, total, page, size);
    }
}
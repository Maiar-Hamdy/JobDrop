using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Geography;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using JobDrop.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Discovery;

public sealed record DiscoverySearchCommand(DiscoveryFilter Filter) : IRequest<SearchContextDto>;

public sealed class DiscoverySearchValidator : AbstractValidator<DiscoverySearchCommand>
{
    public DiscoverySearchValidator()
    {
        When(x => x.Filter.CommuteMode.HasValue, () =>
        {
            RuleFor(x => x.Filter.OriginLon).NotNull();
            RuleFor(x => x.Filter.OriginLat).NotNull();
        });
        RuleFor(x => x.Filter.SalaryMin)
            .Must((cmd, min) => !min.HasValue || !cmd.Filter.SalaryMax.HasValue || min <= cmd.Filter.SalaryMax)
            .WithMessage("SalaryMin must be <= SalaryMax");
    }
}

public sealed class DiscoverySearchHandler(
    IApplicationDbContext db,
    ICommuteRouter router,
    IDateTimeProvider clock)
    : IRequestHandler<DiscoverySearchCommand, SearchContextDto>
{
    public async Task<SearchContextDto> Handle(DiscoverySearchCommand r, CancellationToken ct)
    {
        var f = r.Filter;
        var now = clock.UtcNow;

        var baseQuery =
            from j in db.Jobs.AsNoTracking()
            join c in db.Companies.AsNoTracking() on j.CompanyId equals c.Id
            join o in db.CompanyOffices.AsNoTracking() on j.OfficeId equals o.Id
            where j.Status == JobStatus.Published
            select new { j, c, o };

        if (f.WorkModes is { Length: > 0 })
        {
            var modes = f.WorkModes.Select(m => Enum.Parse<WorkMode>(m)).ToArray();
            baseQuery = baseQuery.Where(x => modes.Contains(x.j.WorkMode));
        }
        if (f.VerifiedCompaniesOnly)
            baseQuery = baseQuery.Where(x => (int)x.c.VerificationLevel >= (int)VerificationLevel.TradeLicense);
        if (f.SalaryMin.HasValue)
            baseQuery = baseQuery.Where(x => x.j.SalaryMax == null || x.j.SalaryMax >= f.SalaryMin);
        if (f.SalaryMax.HasValue)
            baseQuery = baseQuery.Where(x => x.j.SalaryMin == null || x.j.SalaryMin <= f.SalaryMax);

        var rows = await baseQuery.Take(500).ToListAsync(ct);

        // Optional commute matrix via GIS port
        Dictionary<string, CommuteLeg> matrix = new();
        if (f.CommuteMode.HasValue && f.OriginLon.HasValue && f.OriginLat.HasValue && rows.Count > 0)
        {
            var origin = new[] { (f.OriginLon.Value, f.OriginLat.Value) };
            var dest = rows.Select(x => (x.o.Location.X, x.o.Location.Y)).Distinct().ToArray();
            try { matrix = (await router.GetMatrixAsync(origin, dest, f.CommuteMode.Value, now, ct)).ToDictionary(k => k.Key, v => v.Value); }
            catch { matrix = new(); } // graceful degradation per GIS provider availability
        }

        var cards = new List<JobCardDto>();
        foreach (var x in rows)
        {
            int? duration = null;
            var beyondTime = false; var beyondRadius = false;

            if (f.OriginLon.HasValue && f.OriginLat.HasValue)
            {
                var key = $"{f.OriginLon.Value},{f.OriginLat.Value}|{x.o.Location.X},{x.o.Location.Y}";
                if (matrix.TryGetValue(key, out var leg) && leg.Status == "Available") duration = leg.DurationSeconds;
            }

            if (!f.ShowBeyondCommuteLimit)
            {
                if (f.CommuteMode.HasValue && f.MaxDurationMinutes.HasValue)
                {
                    if (duration == null) beyondTime = true;
                    else if (duration > f.MaxDurationMinutes.Value * 60) beyondTime = true;
                }
                if (f.RadiusKm.HasValue && f.OriginLon.HasValue && f.OriginLat.HasValue)
                {
                    var dx = (x.o.Location.X - f.OriginLon.Value) * 111_000 * Math.Cos(f.OriginLat.Value * Math.PI / 180);
                    var dy = (x.o.Location.Y - f.OriginLat.Value) * 111_000;
                    var meters = Math.Sqrt(dx * dx + dy * dy);
                    if (meters > f.RadiusKm.Value * 1000) beyondRadius = true;
                }
                if (beyondTime || beyondRadius) continue;
            }

            // Deterministic pseudo-match score (real scoring lives in Phase 3 when full profile data lands)
            var score = MatchScoringEngine.Score(new MatchScoringEngine.Inputs(
                SkillMatch: 0.75m, ExperienceMatch: 0.70m, EducationMatch: 0.60m,
                CompensationMatch: 0.65m, CommuteEfficiency: duration == null ? 0.5m : Math.Max(0, 1m - (decimal)duration.Value / (48 * 3600m))));

            cards.Add(new JobCardDto(
                x.j.Id, x.j.Title, x.c.DisplayName,
                x.j.SalaryMin, x.j.SalaryMax, x.j.SalaryCurrency,
                x.j.WorkMode.ToString(),
                (int)x.c.VerificationLevel >= (int)VerificationLevel.TradeLicense,
                duration, beyondTime, beyondRadius, score, x.j.PublishedAtUtc));
        }

        // Sort
        cards = f.Sort switch
        {
            SortOrder.CommuteAsc  => cards.OrderBy(c => c.CommuteDurationSeconds ?? int.MaxValue).ToList(),
            SortOrder.PublishedDesc => cards.OrderByDescending(c => c.PublishedAtUtc).ToList(),
            SortOrder.MatchAsc    => cards.OrderBy(c => c.MatchScore).ToList(),
            _                     => cards.OrderByDescending(c => c.MatchScore).ToList()
        };

        return new SearchContextDto(Guid.NewGuid(), cards.Count, cards, now);
    }
}
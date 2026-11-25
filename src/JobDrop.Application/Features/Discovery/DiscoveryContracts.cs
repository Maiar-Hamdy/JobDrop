using JobDrop.Domain.Enums;
namespace JobDrop.Application.Features.Discovery;

public sealed record DiscoveryFilter(
    string[]? TargetRoles, Guid[]? SkillIds, Guid[]? SectorIds,
    string[]? WorkModes,
    double? OriginLon, double? OriginLat,
    TravelMode? CommuteMode, int? MaxDurationMinutes, int? RadiusKm,
    decimal? SalaryMin, decimal? SalaryMax, string? SalaryCurrency,
    bool VerifiedCompaniesOnly, bool ShowBeyondCommuteLimit,
    SortOrder Sort);

public sealed record JobCardDto(
    Guid JobId, string Title, string CompanyDisplayName,
    decimal? SalaryMin, decimal? SalaryMax, string? SalaryCurrency,
    string WorkMode, bool VerifiedCompany,
    int? CommuteDurationSeconds, bool BeyondTime, bool BeyondRadius,
    decimal MatchScore, DateTime? PublishedAtUtc);

public sealed record SearchContextDto(
    Guid SearchContextId, int TotalCount,
    IReadOnlyList<JobCardDto> Cards, DateTime EvaluatedAtUtc);
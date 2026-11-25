namespace JobDrop.Application.Features.Candidates;

public sealed record ProfileDto(
    Guid Id, string? Headline, string? Summary, string? PreferredWorkModes,
    string SharingProtocolJson, string? DropThemeCode, byte[]? RowVersion);

public sealed record CompletenessDto(
    int OverallPercent, int HeadlinePercent, int SummaryPercent,
    int StartingPointsPercent, int SkillsPercent, int ExperiencePercent);
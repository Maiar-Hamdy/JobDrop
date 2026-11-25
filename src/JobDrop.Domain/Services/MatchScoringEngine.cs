namespace JobDrop.Domain.Services;

/// <summary>
/// Pure deterministic scoring — no DB, no IO. Biased-free: no age/gender/photo/nationality.
/// Weights normalized to 0..1 range so caller multiplies by 100.
/// </summary>
public static class MatchScoringEngine
{
    public sealed record Inputs(
        decimal SkillMatch,       // 0..1
        decimal ExperienceMatch,  // 0..1
        decimal EducationMatch,   // 0..1
        decimal CompensationMatch,// 0..1
        decimal CommuteEfficiency // 0..1
    );

    public static decimal Score(in Inputs i)
    {
        var weighted =
            (i.SkillMatch        * 0.35m) +
            (i.ExperienceMatch   * 0.25m) +
            (i.CommuteEfficiency * 0.20m) +
            (i.CompensationMatch * 0.15m) +
            (i.EducationMatch    * 0.05m);
        return Math.Round(weighted * 100m, 2);
    }
}
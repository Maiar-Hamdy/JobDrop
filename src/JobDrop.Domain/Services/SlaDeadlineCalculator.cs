namespace JobDrop.Domain.Services;

/// <summary>
/// Pure SLA deadline calculator. Phase 1 default = strict 48h elapsed UTC.
/// Market policy may later switch to working-hours math from CalendarSnapshot.
/// </summary>
public static class SlaDeadlineCalculator
{
    public const int SlaHours = 48;
    public static DateTime ComputeDueAtUtc(DateTime deliveredAtUtc, string policy = "Strict48hElapsed")
        => policy switch
        {
            "WorkingHours48h" => deliveredAtUtc.AddHours(SlaHours), // TODO Sprint 3+ calendar-aware
            _                 => deliveredAtUtc.AddHours(SlaHours)
        };
}
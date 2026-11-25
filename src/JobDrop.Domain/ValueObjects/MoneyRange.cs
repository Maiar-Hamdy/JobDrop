namespace JobDrop.Domain.ValueObjects;
public readonly record struct MoneyRange(decimal? Min, decimal? Max, string Currency, string Period)
{
    public bool Overlaps(MoneyRange other)
    {
        if (Min is null && Max is null) return true;
        var aMin = Min ?? 0; var aMax = Max ?? decimal.MaxValue;
        var bMin = other.Min ?? 0; var bMax = other.Max ?? decimal.MaxValue;
        return aMin <= bMax && bMin <= aMax;
    }
}
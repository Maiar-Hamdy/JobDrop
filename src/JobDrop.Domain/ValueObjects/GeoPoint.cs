namespace JobDrop.Domain.ValueObjects;

/// <summary>Immutable WGS84 point. Longitude/Latitude (SRID 4326).</summary>
public readonly record struct GeoPoint(double Longitude, double Latitude)
{
    public bool IsValid()
        => !double.IsNaN(Longitude) && !double.IsInfinity(Longitude)
        && !double.IsNaN(Latitude)  && !double.IsInfinity(Latitude)
        && Longitude is >= -180 and <= 180
        && Latitude  is >= -90  and <= 90
        && !(Longitude == 0 && Latitude == 0);
}
using JobDrop.Domain.Enums;

namespace JobDrop.Application.Contracts.Geography;

public sealed record GeoPlace(string PlaceId, string DisplayName, double Longitude, double Latitude);
public sealed record CommuteLeg(int DurationSeconds, int DistanceMeters, string Status, string Source, DateTime AsOfUtc);

public interface IGeocodingProvider
{
    Task<GeoPlace?> ResolveAsync(string placeReference, string marketCode, CancellationToken ct);
}

public interface ICommuteRouter
{
    Task<IReadOnlyDictionary<string, CommuteLeg>> GetMatrixAsync(
        IReadOnlyList<(double lon, double lat)> origins,
        IReadOnlyList<(double lon, double lat)> destinations,
        TravelMode mode,
        DateTime departureUtc,
        CancellationToken ct);
}

public interface IIsochroneProvider
{
    /// <summary>Returns GeoJSON MultiPolygon string (RFC 7946) or null if market unsupported.</summary>
    Task<string?> GetIsochroneAsync(double lon, double lat, TravelMode mode, int maxMinutes, CancellationToken ct);
}

public interface IOperatingAreaQuery
{
    Task<bool> ContainsAsync(double lon, double lat, CancellationToken ct);
    Task<string?> GetMarketCodeAsync(double lon, double lat, CancellationToken ct);
}
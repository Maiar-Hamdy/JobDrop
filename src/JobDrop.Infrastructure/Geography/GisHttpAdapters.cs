using System.Net.Http.Json;
using System.Text.Json;
using JobDrop.Application.Contracts.Geography;
using JobDrop.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobDrop.Infrastructure.Geography;

/// <summary>
/// HTTP adapters targeting the GIS team service contract.
/// Expected endpoints (adjust in one file when GIS contract finalizes):
///   POST {BaseUrl}/v1/geocode             { reference, market } -> GeoPlace
///   POST {BaseUrl}/v1/commute/matrix      { origins[], destinations[], mode, departureUtc }
///   POST {BaseUrl}/v1/isochrone           { lon, lat, mode, maxMinutes }
///   POST {BaseUrl}/v1/operating-area/contains { lon, lat }
///   POST {BaseUrl}/v1/operating-area/market   { lon, lat }
/// </summary>
public sealed class GisHttpAdapters(IOptions<GeographyOptions> opts, HttpClient http, ILogger<GisHttpAdapters> logger)
    : IGeocodingProvider, ICommuteRouter, IIsochroneProvider, IOperatingAreaQuery
{
    private readonly GeographyOptions _o = opts.Value;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private void Auth() => http.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", _o.ApiKey);

    public async Task<GeoPlace?> ResolveAsync(string placeReference, string marketCode, CancellationToken ct)
    {
        Auth();
        var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/geocode", new { reference = placeReference, market = marketCode }, Json, ct);
        if (!resp.IsSuccessStatusCode) { logger.LogWarning("Geocode failed {Status}", resp.StatusCode); return null; }
        return await resp.Content.ReadFromJsonAsync<GeoPlace>(Json, ct);
    }

    public async Task<IReadOnlyDictionary<string, CommuteLeg>> GetMatrixAsync(
        IReadOnlyList<(double lon, double lat)> origins,
        IReadOnlyList<(double lon, double lat)> destinations,
        TravelMode mode,
        DateTime departureUtc,
        CancellationToken ct)
    {
        Auth();
        var body = new
        {
            origins = origins.Select(o => new { lon = o.lon, lat = o.lat }),
            destinations = destinations.Select(d => new { lon = d.lon, lat = d.lat }),
            mode = mode.ToString(),
            departureUtc
        };
        var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/commute/matrix", body, Json, ct);
        if (!resp.IsSuccessStatusCode) { logger.LogWarning("Commute matrix failed {Status}", resp.StatusCode); return new Dictionary<string, CommuteLeg>(); }
        var legs = await resp.Content.ReadFromJsonAsync<Dictionary<string, CommuteLeg>>(Json, ct);
        return legs ?? new Dictionary<string, CommuteLeg>();
    }

    public async Task<string?> GetIsochroneAsync(double lon, double lat, TravelMode mode, int maxMinutes, CancellationToken ct)
    {
        Auth();
        var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/isochrone", new { lon, lat, mode = mode.ToString(), maxMinutes }, Json, ct);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsStringAsync(ct) : null;
    }

    public async Task<bool> ContainsAsync(double lon, double lat, CancellationToken ct)
    {
        Auth();
        var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/operating-area/contains", new { lon, lat }, Json, ct);
        return resp.IsSuccessStatusCode && await resp.Content.ReadFromJsonAsync<bool>(Json, ct);
    }

    public async Task<string?> GetMarketCodeAsync(double lon, double lat, CancellationToken ct)
    {
        Auth();
        var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/operating-area/market", new { lon, lat }, Json, ct);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<string>(Json, ct) : null;
    }
}
using System.ComponentModel.DataAnnotations;
namespace JobDrop.Infrastructure.Geography;

public sealed class GeographyOptions
{
    /// <summary>GIS team's base URL (e.g. https://gis.jobdrop.internal).</summary>
    [Required] public string BaseUrl { get; set; } = default!;
    [Required] public string ApiKey { get; set; } = default!;
    public int TimeoutSeconds { get; set; } = 8;
}
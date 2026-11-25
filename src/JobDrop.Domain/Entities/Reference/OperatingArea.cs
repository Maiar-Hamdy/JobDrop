using JobDrop.Domain.Common;
using NetTopologySuite.Geometries;
namespace JobDrop.Domain.Entities.Reference;
public sealed class OperatingArea : BaseEntity
{
    public string MarketCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string TimeZoneId { get; set; } = default!;
    public MultiPolygon Boundary { get; set; } = default!;
    public Point CenterPoint { get; set; } = default!;
    public string BoundarySource { get; set; } = "NotConfigured";
    public bool IsActive { get; set; } = true;
}
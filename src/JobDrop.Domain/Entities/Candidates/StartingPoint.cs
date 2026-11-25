using JobDrop.Domain.Common;
using NetTopologySuite.Geometries;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class StartingPoint : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string Label { get; set; } = default!;
    public string AnchorType { get; set; } = "Primary"; // Primary|Secondary|Coworking
    public Point Location { get; set; } = default!;     // geography SRID 4326
    public string PrivacyZoneLabel { get; set; } = "ApproximateArea";
    public bool IsActiveOrigin { get; set; } = true;
}
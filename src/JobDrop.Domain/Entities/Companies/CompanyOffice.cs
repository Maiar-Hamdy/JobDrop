using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
using NetTopologySuite.Geometries;
namespace JobDrop.Domain.Entities.Companies;
public sealed class CompanyOffice : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid OperatingAreaId { get; set; }
    public string Nickname { get; set; } = default!;
    public OfficeType OfficeType { get; set; } = OfficeType.HQ;
    public Point Location { get; set; } = default!;  // geography SRID 4326
    public int GeofenceRadiusMeters { get; set; } = 500;
    public string? TransitAnchorText { get; set; }
    public VerificationLevel VerificationLevel { get; set; } = VerificationLevel.Unverified;
    public int LocationVersion { get; set; } = 1;
}
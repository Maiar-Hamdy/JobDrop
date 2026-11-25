using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Companies;
public sealed class Company : AggregateRoot
{
    public string LegalName { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? ThemeColorHex { get; set; }
    public string? CommercialLicenseNo { get; set; }
    public VerificationLevel VerificationLevel { get; set; } = VerificationLevel.Unverified;
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }
}
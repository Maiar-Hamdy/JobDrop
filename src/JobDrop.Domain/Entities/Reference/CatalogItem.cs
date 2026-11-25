using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Reference;
public sealed class CatalogItem : BaseEntity
{
    public string Category { get; set; } = default!;   // Role|Sector|Skill|Language|Institution|ReasonCode
    public string Code { get; set; } = default!;
    public string NameEn { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
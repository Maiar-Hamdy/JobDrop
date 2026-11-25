using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Billing;
public sealed class Plan : BaseEntity
{
    public string Code { get; set; } = default!;          // free | pro | pro-plus
    public string DisplayName { get; set; } = default!;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "AED";
    public string Period { get; set; } = "monthly";       // monthly | annual
    public string MarketCode { get; set; } = "AE";
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string EntitlementsJson { get; set; } = "{}";  // { dailyDrops, reverseDrops, talentSearch }
}
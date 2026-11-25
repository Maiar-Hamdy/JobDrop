using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.ReverseDrops;
public sealed class ReverseDropPool : BaseEntity
{
    public Guid CompanyId { get; set; }
    public DateTime CycleStartUtc { get; set; }
    public DateTime CycleEndUtc { get; set; }
    public int MonthlyRemaining { get; set; }
    public int TopUpRemaining { get; set; }
    public byte[]? RowVersion { get; set; }
}
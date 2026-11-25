using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Jobs;
public sealed class JobDailyIntake : BaseEntity
{
    public Guid JobId { get; set; }
    public DateOnly MarketDate { get; set; }
    public int DeliveredCount { get; set; }
    public int CapLimit { get; set; }
    public byte[]? RowVersion { get; set; }
}
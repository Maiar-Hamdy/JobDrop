using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Jobs;
public sealed class Job : AggregateRoot
{
    public Guid CompanyId { get; set; }
    public Guid OfficeId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Title { get; set; } = default!;
    public string? ThemeColorHex { get; set; }
    public WorkMode WorkMode { get; set; } = WorkMode.OnSite;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public bool IsSalaryNegotiable { get; set; }
    public int DailyIntakeCap { get; set; } = 10;
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }
}
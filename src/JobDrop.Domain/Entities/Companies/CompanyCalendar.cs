using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Companies;
public sealed class CompanyCalendar : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string WorkingDaysCsv { get; set; } = "Mon,Tue,Wed,Thu,Fri";
    public int WorkStartMinute { get; set; } = 9 * 60;
    public int WorkEndMinute { get; set; } = 18 * 60;
    public string TimeZoneId { get; set; } = "Asia/Dubai";
}
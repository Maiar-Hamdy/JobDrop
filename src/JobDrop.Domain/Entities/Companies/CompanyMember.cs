using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Companies;
public sealed class CompanyMember : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public CompanyMemberRole Role { get; set; } = CompanyMemberRole.Viewer;
    public string? DepartmentScope { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
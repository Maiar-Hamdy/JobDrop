using MediatR;
namespace JobDrop.Application.Features.Companies;

public sealed record CompanyDto(Guid Id, string LegalName, string DisplayName, int VerificationLevel, bool IsActive);
public sealed record OfficeDto(Guid Id, string Nickname, string OfficeType, double Lon, double Lat, int GeofenceRadiusMeters, int VerificationLevel, int LocationVersion);
public sealed record CreateCompanyCmd(string LegalName, string DisplayName, string? ThemeColorHex) : MediatR.IRequest<Guid>;
public sealed record CreateOfficeCmd(Guid CompanyId, string Nickname, string OfficeType, double Lon, double Lat, int GeofenceRadiusMeters) : MediatR.IRequest<Guid>;
public sealed record ListOfficesQuery(Guid CompanyId) : MediatR.IRequest<IReadOnlyList<OfficeDto>>;
public sealed record CalendarDto(string WorkingDaysCsv, int WorkStartMinute, int WorkEndMinute, string TimeZoneId);
public sealed record GetCalendarQuery(Guid CompanyId) : MediatR.IRequest<CalendarDto>;
public sealed record UpsertCalendarCmd(Guid CompanyId, string WorkingDaysCsv, int WorkStartMinute, int WorkEndMinute, string TimeZoneId) : MediatR.IRequest<Unit>;
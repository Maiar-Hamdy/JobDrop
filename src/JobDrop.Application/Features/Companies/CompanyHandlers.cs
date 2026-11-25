using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Companies;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace JobDrop.Application.Features.Companies;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCmd>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateCompanyHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<CreateCompanyCmd, Guid>
{
    public async Task<Guid> Handle(CreateCompanyCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var c = new Company { LegalName = r.LegalName, DisplayName = r.DisplayName, ThemeColorHex = r.ThemeColorHex };
        db.Companies.Add(c);
        db.CompanyMembers.Add(new CompanyMember { CompanyId = c.Id, UserId = uid, Role = CompanyMemberRole.Owner });
        db.CompanyCalendars.Add(new CompanyCalendar { CompanyId = c.Id });
        await db.SaveChangesAsync(ct);
        return c.Id;
    }
}

public sealed class CreateOfficeValidator : AbstractValidator<CreateOfficeCmd>
{
    public CreateOfficeValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OfficeType).Must(v => v is "HQ" or "SatelliteHub" or "RecruitingBooth");
        RuleFor(x => x.Lon).InclusiveBetween(-180, 180);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
    }
}

public sealed class CreateOfficeHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<CreateOfficeCmd, Guid>
{
    public async Task<Guid> Handle(CreateOfficeCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var isMember = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == r.CompanyId && m.UserId == uid, ct);
        if (!isMember) throw new Domain.Common.DomainRuleException("Not a member of the company.");

        var office = new CompanyOffice
        {
            CompanyId = r.CompanyId,
            OperatingAreaId = Guid.Empty,
            Nickname = r.Nickname,
            OfficeType = Enum.Parse<OfficeType>(r.OfficeType),
            Location = new Point(r.Lon, r.Lat) { SRID = 4326 },
            GeofenceRadiusMeters = r.GeofenceRadiusMeters
        };
        db.CompanyOffices.Add(office);
        await db.SaveChangesAsync(ct);
        return office.Id;
    }
}

public sealed class ListOfficesHandler(IApplicationDbContext db)
    : IRequestHandler<ListOfficesQuery, IReadOnlyList<OfficeDto>>
{
    public async Task<IReadOnlyList<OfficeDto>> Handle(ListOfficesQuery r, CancellationToken ct)
        => await db.CompanyOffices.AsNoTracking()
            .Where(o => o.CompanyId == r.CompanyId)
            .Select(o => new OfficeDto(o.Id, o.Nickname, o.OfficeType.ToString(),
                o.Location.X, o.Location.Y, o.GeofenceRadiusMeters,
                (int)o.VerificationLevel, o.LocationVersion))
            .ToListAsync(ct);
}

public sealed class GetCalendarHandler(IApplicationDbContext db)
    : IRequestHandler<GetCalendarQuery, CalendarDto>
{
    public async Task<CalendarDto> Handle(GetCalendarQuery r, CancellationToken ct)
    {
        var cal = await db.CompanyCalendars.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == r.CompanyId, ct)
            ?? throw new Domain.Common.NotFoundException("Calendar not found.");
        return new CalendarDto(cal.WorkingDaysCsv, cal.WorkStartMinute, cal.WorkEndMinute, cal.TimeZoneId);
    }
}

public sealed class UpsertCalendarHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertCalendarCmd, Unit>
{
    public async Task<Unit> Handle(UpsertCalendarCmd r, CancellationToken ct)
    {
        var cal = await db.CompanyCalendars.FirstOrDefaultAsync(c => c.CompanyId == r.CompanyId, ct);
        if (cal == null)
        {
            cal = new CompanyCalendar { CompanyId = r.CompanyId };
            db.CompanyCalendars.Add(cal);
        }
        cal.WorkingDaysCsv = r.WorkingDaysCsv;
        cal.WorkStartMinute = r.WorkStartMinute;
        cal.WorkEndMinute = r.WorkEndMinute;
        cal.TimeZoneId = r.TimeZoneId;
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
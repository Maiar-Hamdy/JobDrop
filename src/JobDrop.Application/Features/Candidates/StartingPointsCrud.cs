using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Candidates;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace JobDrop.Application.Features.Candidates;

public sealed record StartingPointDto(Guid Id, string Label, string AnchorType, double Lon, double Lat, bool IsActiveOrigin);

public sealed record ListStartingPointsQuery : IRequest<IReadOnlyList<StartingPointDto>>;
public sealed record AddStartingPointCmd(string Label, double Lon, double Lat, string AnchorType) : IRequest<Guid>;
public sealed record DeleteStartingPointCmd(Guid Id) : IRequest<Unit>;

public sealed class ListSpHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<ListStartingPointsQuery, IReadOnlyList<StartingPointDto>>
{
    public async Task<IReadOnlyList<StartingPointDto>> Handle(ListStartingPointsQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        return await db.StartingPoints.AsNoTracking()
            .Where(s => s.CandidateProfileId == profileId)
            .Select(s => new StartingPointDto(s.Id, s.Label, s.AnchorType, s.Location.X, s.Location.Y, s.IsActiveOrigin))
            .ToListAsync(ct);
    }
}

public sealed class AddStartingPointValidator : AbstractValidator<AddStartingPointCmd>
{
    public AddStartingPointValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Lon).InclusiveBetween(-180, 180);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.AnchorType).Must(v => v is "Primary" or "Secondary" or "Coworking");
    }
}

public sealed class AddStartingPointHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<AddStartingPointCmd, Guid>
{
    public async Task<Guid> Handle(AddStartingPointCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (profileId == Guid.Empty) throw new Domain.Common.NotFoundException("Profile missing.");
        var sp = new StartingPoint
        {
            CandidateProfileId = profileId,
            Label = r.Label,
            AnchorType = r.AnchorType,
            Location = new Point(r.Lon, r.Lat) { SRID = 4326 }
        };
        db.StartingPoints.Add(sp);
        await db.SaveChangesAsync(ct);
        return sp.Id;
    }
}

public sealed class DeleteStartingPointHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<DeleteStartingPointCmd, Unit>
{
    public async Task<Unit> Handle(DeleteStartingPointCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        var sp = await db.StartingPoints.FirstOrDefaultAsync(s => s.Id == r.Id && s.CandidateProfileId == profileId, ct)
            ?? throw new Domain.Common.NotFoundException("Starting point not found.");
        db.StartingPoints.Remove(sp);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
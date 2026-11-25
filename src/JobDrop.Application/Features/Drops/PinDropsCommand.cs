using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Candidates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Drops;

public sealed record PinDropsCommand(IReadOnlyList<Guid> ApplicationIds) : IRequest<Unit>;

public sealed class PinDropsValidator : AbstractValidator<PinDropsCommand>
{
    public PinDropsValidator() => RuleFor(x => x.ApplicationIds).Must(a => a.Count <= 3);
}

public sealed class PinDropsHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<PinDropsCommand, Unit>
{
    public async Task<Unit> Handle(PinDropsCommand r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);

        var existing = await db.PinnedDrops.Where(p => p.CandidateProfileId == profileId).ToListAsync(ct);
        foreach (var e in existing) db.PinnedDrops.Remove(e);

        var order = 1;
        foreach (var appId in r.ApplicationIds.Distinct())
        {
            var owns = await db.Applications.AsNoTracking()
                .AnyAsync(a => a.Id == appId && a.CandidateProfileId == profileId, ct);
            if (!owns) throw new Domain.Common.DomainRuleException("Cannot pin an application you don't own.");
            db.PinnedDrops.Add(new PinnedDrop { CandidateProfileId = profileId, ApplicationId = appId, PinOrder = order++ });
        }
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
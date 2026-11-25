using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Candidates;

public sealed record GetProfileQuery : IRequest<ProfileDto>;

public sealed class GetProfileHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var userId = user.UserId ?? throw new UnauthorizedAccessException();
        var p = await db.CandidateProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new ProfileDto(x.Id, x.Headline, x.Summary, x.PreferredWorkModes,
                                        x.SharingProtocolJson, x.DropThemeCode, x.RowVersion))
            .FirstOrDefaultAsync(ct)
            ?? throw new Domain.Common.NotFoundException("Profile not found.");
        return p;
    }
}
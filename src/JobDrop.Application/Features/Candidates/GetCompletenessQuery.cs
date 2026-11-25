using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Candidates;

public sealed record GetCompletenessQuery : IRequest<CompletenessDto>;

public sealed class GetCompletenessHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetCompletenessQuery, CompletenessDto>
{
    public async Task<CompletenessDto> Handle(GetCompletenessQuery request, CancellationToken ct)
    {
        var userId = user.UserId ?? throw new UnauthorizedAccessException();
        var p = await db.CandidateProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct)
            ?? throw new Domain.Common.NotFoundException("Profile not found.");

        var hasStarting = await db.StartingPoints.AnyAsync(s => s.CandidateProfileId == p.Id && s.IsActiveOrigin, ct);

        var headline = string.IsNullOrWhiteSpace(p.Headline) ? 0 : 100;
        var summary  = string.IsNullOrWhiteSpace(p.Summary) ? 0 : (p.Summary!.Length >= 100 ? 100 : 50);
        var sp       = hasStarting ? 100 : 0;
        var overall  = (headline + summary + sp) / 3;

        return new CompletenessDto(overall, headline, summary, sp, 0, 0);
    }
}
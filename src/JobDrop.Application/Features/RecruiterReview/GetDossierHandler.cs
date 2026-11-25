using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Enums;
using JobDrop.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.RecruiterReview;

public sealed class GetDossierHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<GetDossierQuery, DossierDto>
{
    public async Task<DossierDto> Handle(GetDossierQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();

        var app = await db.Applications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == r.ApplicationId, ct)
            ?? throw new Domain.Common.NotFoundException("Application not found.");
        var isRecruiter = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == app.CompanyId && m.UserId == uid, ct);
        if (!isRecruiter) throw new UnauthorizedAccessException("Only assigned recruiters may view dossiers.");

        var snap = await db.ApplicationSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ApplicationId == app.Id, ct);

        // Disclosure: only after InviteToInterview or OfferExtended
        var disclosed = app.Status == ApplicationStatus.InterviewScheduled
                        || app.Status == ApplicationStatus.OfferExtended;

        var candidateCode = BlindDossierProjector.CandidateCode(app.CandidateProfileId);

        return new DossierDto(
            ApplicationId: app.Id,
            CandidateCode: candidateCode,
            MaskedName: disclosed ? "Full profile available" : BlindDossierProjector.MaskName(null),
            SkillBadges: Array.Empty<string>(),
            MatchScore: snap?.MatchScore ?? 0m,
            Status: app.Status.ToString(),
            IsDisclosed: disclosed,
            FullName: disclosed ? "Full Name" : null,
            Phone: disclosed ? "+971-***" : null,
            RedactedCvKey: snap?.RedactedResumeKey,
            OriginalCvKey: disclosed ? snap?.RedactedResumeKey : null);
    }
}
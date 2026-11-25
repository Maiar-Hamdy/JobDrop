using MediatR;
namespace JobDrop.Application.Features.RecruiterReview;

public sealed record DossierDto(
    Guid ApplicationId, string CandidateCode, string MaskedName,
    string[] SkillBadges, decimal MatchScore, string Status,
    bool IsDisclosed, string? FullName, string? Phone, string? RedactedCvKey, string? OriginalCvKey);

public sealed record GetDossierQuery(Guid ApplicationId) : MediatR.IRequest<DossierDto>;
public sealed record DossierActionCmd(Guid ApplicationId, string Action, string? RejectionReasonCode, string? Note) : MediatR.IRequest<Unit>;
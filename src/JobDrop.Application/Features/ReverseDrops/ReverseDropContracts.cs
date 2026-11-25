using MediatR;
namespace JobDrop.Application.Features.ReverseDrops;

public sealed record DispatchReverseDropCmd(
    Guid JobId, Guid CandidateProfileId, string? OfferDocumentKey, string? DispatchNote,
    bool IsSalaryNegotiable, string IdempotencyKey) : MediatR.IRequest<Guid>;

public sealed record RespondReverseDropCmd(Guid ReverseDropId, string Action) : MediatR.IRequest<Unit>;

public sealed record ReverseDropDto(Guid Id, Guid CompanyId, Guid JobId, Guid CandidateProfileId,
    string Status, DateTime ExpiresAtUtc, string? DispatchNote);
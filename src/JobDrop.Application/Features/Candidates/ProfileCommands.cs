using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Candidates;

public sealed record UpdateProfileCommand(string? Headline, string? Summary) : IRequest<Unit>;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Summary).MaximumLength(4000);
    }
}

public sealed class UpdateProfileHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateProfileCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var profile = await db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Profile not found.");

        profile.Headline = request.Headline;
        profile.Summary = request.Summary;
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
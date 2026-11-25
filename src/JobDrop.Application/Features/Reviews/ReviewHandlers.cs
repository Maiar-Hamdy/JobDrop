using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Reviews;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Reviews;

public sealed class SubmitReviewValidator : AbstractValidator<SubmitReviewCmd>
{
    public SubmitReviewValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.TargetType).Must(t => t is "Company" or "Candidate");
    }
}

public sealed class SubmitReviewHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<SubmitReviewCmd, Guid>
{
    public async Task<Guid> Handle(SubmitReviewCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var app = await db.Applications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == r.ApplicationId, ct)
            ?? throw new Domain.Common.NotFoundException("Application not found.");

        // Eligibility: terminal status + verified interaction beyond Viewed
        var terminal = app.Status is ApplicationStatus.InterviewScheduled
                        or ApplicationStatus.OfferExtended
                        or ApplicationStatus.Rejected;
        if (!terminal) throw new Domain.Common.DomainRuleException("Review not permitted until interaction finalised.");

        var already = await db.Set<Review>().AnyAsync(x => x.ApplicationId == app.Id && x.ReviewerUserId == uid, ct);
        if (already) throw new Domain.Common.DomainRuleException("You already reviewed this application.");

        var targetId = r.TargetType == "Company" ? app.CompanyId : app.CandidateProfileId;
        var review = new Review
        {
            ApplicationId = app.Id,
            ReviewerUserId = uid,
            TargetType = r.TargetType,
            TargetId = targetId,
            Rating = r.Rating,
            Comment = r.Comment,
            Status = "Pending",
            CreatedAtUtc = clock.UtcNow
        };
        db.Set<Review>().Add(review);
        await db.SaveChangesAsync(ct);
        return review.Id;
    }
}

public sealed class ReportReviewValidator : AbstractValidator<ReportReviewCmd>
{
    public ReportReviewValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class ReportReviewHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<ReportReviewCmd, Guid>
{
    public async Task<Guid> Handle(ReportReviewCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var report = new ReviewReport
        {
            ReviewId = r.ReviewId,
            ReporterUserId = uid,
            Reason = r.Reason,
            CreatedAtUtc = clock.UtcNow
        };
        db.Set<ReviewReport>().Add(report);
        await db.SaveChangesAsync(ct);
        return report.Id;
    }
}
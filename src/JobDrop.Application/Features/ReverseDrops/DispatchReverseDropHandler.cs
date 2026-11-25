using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.ReverseDrops;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.ReverseDrops;

public sealed class DispatchReverseDropValidator : AbstractValidator<DispatchReverseDropCmd>
{
    public DispatchReverseDropValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.CandidateProfileId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(120);
    }
}

public sealed class DispatchReverseDropHandler(IApplicationDbContext db, ICurrentUser user, IDateTimeProvider clock)
    : IRequestHandler<DispatchReverseDropCmd, Guid>
{
    public async Task<Guid> Handle(DispatchReverseDropCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == r.JobId, ct)
            ?? throw new Domain.Common.NotFoundException("Job not found.");
        var isRecruiter = await db.CompanyMembers.AsNoTracking()
            .AnyAsync(m => m.CompanyId == job.CompanyId && m.UserId == uid, ct);
        if (!isRecruiter) throw new UnauthorizedAccessException();

        var existing = await db.ReverseDrops.FirstOrDefaultAsync(x => x.IdempotencyKey == r.IdempotencyKey, ct);
        if (existing != null) return existing.Id;

        using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var pool = await db.ReverseDropPools.FirstOrDefaultAsync(p => p.CompanyId == job.CompanyId, ct);
            if (pool == null)
            {
                pool = new ReverseDropPool
                {
                    CompanyId = job.CompanyId,
                    CycleStartUtc = new DateTime(clock.UtcNow.Year, clock.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    CycleEndUtc = new DateTime(clock.UtcNow.Year, clock.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
                    MonthlyRemaining = 30
                };
                db.ReverseDropPools.Add(pool);
            }
            if (clock.UtcNow >= pool.CycleEndUtc)
            {
                pool.CycleStartUtc = clock.UtcNow;
                pool.CycleEndUtc = clock.UtcNow.AddDays(30);
                pool.MonthlyRemaining = 30;
            }
            if (pool.MonthlyRemaining <= 0 && pool.TopUpRemaining <= 0)
                throw new Domain.Common.DomainRuleException("Reverse drop pool exhausted.");

            if (pool.MonthlyRemaining > 0) pool.MonthlyRemaining--;
            else pool.TopUpRemaining--;

            var rd = new ReverseDrop
            {
                CompanyId = job.CompanyId,
                JobId = job.Id,
                SenderUserId = uid,
                CandidateProfileId = r.CandidateProfileId,
                OfferDocumentKey = r.OfferDocumentKey,
                DispatchNote = r.DispatchNote,
                IsSalaryNegotiable = r.IsSalaryNegotiable,
                ExpiresAtUtc = clock.UtcNow.AddHours(48),
                IdempotencyKey = r.IdempotencyKey
            };
            db.ReverseDrops.Add(rd);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return rd.Id;
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }
}

public sealed class RespondReverseDropValidator : AbstractValidator<RespondReverseDropCmd>
{
    public RespondReverseDropValidator()
        => RuleFor(x => x.Action).Must(a => a is "accept" or "decline");
}

public sealed class RespondReverseDropHandler(IApplicationDbContext db, ICurrentUser user)
    : IRequestHandler<RespondReverseDropCmd, Unit>
{
    public async Task<Unit> Handle(RespondReverseDropCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var rd = await db.ReverseDrops.FirstOrDefaultAsync(x => x.Id == r.ReverseDropId, ct)
            ?? throw new Domain.Common.NotFoundException("Reverse drop not found.");
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (rd.CandidateProfileId != profileId) throw new UnauthorizedAccessException();

        rd.Status = r.Action == "accept" ? ReverseDropStatus.Accepted : ReverseDropStatus.Declined;
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
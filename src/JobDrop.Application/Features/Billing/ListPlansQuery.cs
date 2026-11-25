using JobDrop.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Billing;

public sealed record ListPlansQuery : IRequest<IReadOnlyList<PlanDto>>;

public sealed class ListPlansHandler(IApplicationDbContext db)
    : IRequestHandler<ListPlansQuery, IReadOnlyList<PlanDto>>
{
    public async Task<IReadOnlyList<PlanDto>> Handle(ListPlansQuery r, CancellationToken ct)
        => await db.Plans.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceAmount)
            .Select(p => new PlanDto(p.Id, p.Code, p.DisplayName, p.PriceAmount, p.Currency, p.Period, p.EntitlementsJson))
            .ToListAsync(ct);
}
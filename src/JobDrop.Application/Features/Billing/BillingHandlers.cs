using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentValidation;
using JobDrop.Application.Common;
using JobDrop.Application.Contracts.Billing;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Domain.Entities.Billing;
using JobDrop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.Application.Features.Billing;

public sealed class CreateCheckoutValidator : AbstractValidator<CreateCheckoutCmd>
{
    public CreateCheckoutValidator() => RuleFor(x => x.PlanId).NotEmpty();
}

public sealed class CreateCheckoutHandler(
    IApplicationDbContext db, ICurrentUser user, IPaymobClient paymob, IDateTimeProvider clock)
    : IRequestHandler<CreateCheckoutCmd, CheckoutDto>
{
    public async Task<CheckoutDto> Handle(CreateCheckoutCmd r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var plan = await db.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == r.PlanId && p.IsActive, ct)
            ?? throw new Domain.Common.NotFoundException("Plan not found.");

        var merchantRef = $"JOB-{clock.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 30);
        var payment = new Payment
        {
            UserId = uid,
            CompanyId = r.CompanyId,
            MerchantReference = merchantRef,
            ProviderOrderId = "", // filled by paymob response
            AmountCents = (long)(plan.PriceAmount * 100),
            Currency = plan.Currency,
            Status = PaymentStatus.Created
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        var result = await paymob.CreateIntentionAsync(payment, plan, ct);
        payment.ProviderOrderId = result.ProviderOrderId;
        payment.ProviderIntentionId = result.ProviderIntentionId;
        payment.Status = PaymentStatus.Pending;
        await db.SaveChangesAsync(ct);

        return new CheckoutDto(payment.Id, result.ProviderOrderId, result.CheckoutUrl, payment.AmountCents, payment.Currency);
    }
}

public sealed class PaymobWebhookValidator : AbstractValidator<PaymobWebhookCmd>
{
    public PaymobWebhookValidator()
    {
        RuleFor(x => x.RawPayload).NotEmpty().MaximumLength(65536);
        RuleFor(x => x.Hmac).NotEmpty();
    }
}

public sealed class PaymobWebhookHandler(
    IApplicationDbContext db, IPaymobClient paymob, IDateTimeProvider clock)
    : IRequestHandler<PaymobWebhookCmd, Unit>
{
    public async Task<Unit> Handle(PaymobWebhookCmd r, CancellationToken ct)
    {
        if (!await paymob.VerifyHmacAsync(r.Hmac, r.RawPayload, ct))
            throw new UnauthorizedAccessException("Invalid HMAC signature.");

        using var doc = JsonDocument.Parse(r.RawPayload);
        var root = doc.RootElement;

        var obj = root.TryGetProperty("obj", out var o) ? o : root;
        var orderId = obj.GetProperty("order").GetProperty("id").GetInt64().ToString();
        var txId = obj.TryGetProperty("id", out var txProp) ? txProp.GetInt64().ToString() : "0";
        var success = obj.TryGetProperty("success", out var s) && s.GetBoolean();
        var isRefunded = obj.TryGetProperty("is_refunded", out var rf) && rf.GetBoolean();
        var isVoided = obj.TryGetProperty("is_voided", out var vd) && vd.GetBoolean();
        var pending = obj.TryGetProperty("pending", out var p) && p.GetBoolean();

        var stateBucket = isRefunded ? "Refunded" : isVoided ? "Voided" : success ? "Success" : pending ? "Pending" : "Failed";

        var dedupKey = $"paymob:{txId}:{stateBucket}";
        var alreadyProcessed = await db.PaymentWebhookEvents.AnyAsync(e => e.EventStateKey == stateBucket && e.ProviderTransactionId == txId, ct);
        if (alreadyProcessed) return Unit.Value;

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.ProviderOrderId == orderId, ct)
            ?? throw new Domain.Common.NotFoundException("Payment not found by order.id.");

        db.PaymentWebhookEvents.Add(new PaymentWebhookEvent
        {
            ProviderTransactionId = txId,
            EventStateKey = stateBucket,
            PayloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(r.RawPayload))),
            ReceivedAtUtc = clock.UtcNow
        });

        switch (stateBucket)
        {
            case "Success":
                payment.Status = PaymentStatus.Paid;
                payment.ProviderTransactionId = txId;
                break;
            case "Refunded":
                payment.Status = PaymentStatus.Refunded;
                break;
            case "Voided":
                payment.Status = PaymentStatus.Voided;
                break;
            case "Failed":
                payment.Status = PaymentStatus.Failed;
                break;
        }

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public sealed class GetEntitlementsHandler(IApplicationDbContext db, ICurrentUser user, IEntitlementResolver resolver)
    : IRequestHandler<GetEntitlementsQuery, EntitlementsDto>
{
    public async Task<EntitlementsDto> Handle(GetEntitlementsQuery r, CancellationToken ct)
    {
        var uid = user.UserId ?? throw new UnauthorizedAccessException();
        var profileId = await db.CandidateProfiles.AsNoTracking()
            .Where(p => p.UserId == uid).Select(p => p.Id).FirstOrDefaultAsync(ct);
        var daily = await resolver.GetDailyDropAllowanceAsync(profileId, ct);
        return new EntitlementsDto("free", daily, 0, Array.Empty<string>());
    }
}
using MediatR;
namespace JobDrop.Application.Features.Billing;

public sealed record PlanDto(Guid Id, string Code, string DisplayName, decimal PriceAmount, string Currency, string Period, string EntitlementsJson);
public sealed record CreateCheckoutCmd(Guid PlanId, Guid? CompanyId) : IRequest<CheckoutDto>;
public sealed record CheckoutDto(Guid PaymentId, string ProviderOrderId, string CheckoutUrl, long AmountCents, string Currency);
public sealed record PaymobWebhookCmd(string RawPayload, string Hmac) : IRequest<Unit>;
public sealed record GetEntitlementsQuery : IRequest<EntitlementsDto>;
public sealed record EntitlementsDto(string PlanCode, int DailyDrops, int MonthlyReverseDrops, string[] Features);
using JobDrop.Domain.Entities.Billing;
namespace JobDrop.Application.Contracts.Billing;

public sealed record CheckoutResult(string ProviderOrderId, string? ProviderIntentionId, string CheckoutUrl);

public interface IPaymobClient
{
    Task<CheckoutResult> CreateIntentionAsync(Payment payment, Plan plan, CancellationToken ct);
    Task<bool> VerifyHmacAsync(string hmac, string rawPayload, CancellationToken ct);
}

public interface IEntitlementResolver
{
    Task<int> GetDailyDropAllowanceAsync(Guid candidateProfileId, CancellationToken ct);
    Task<int> GetMonthlyReverseDropAllowanceAsync(Guid companyId, CancellationToken ct);
    Task<bool> HasProFeatureAsync(Guid userId, string featureKey, CancellationToken ct);
}
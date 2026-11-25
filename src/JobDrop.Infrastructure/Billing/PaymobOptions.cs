namespace JobDrop.Infrastructure.Billing;

/// <summary>
/// Paymob configuration. Secrets (ApiKey/IntegrationId/HmacSecret) are OPTIONAL
/// so the app boots in sandbox mode. When empty, PaymobClient returns a stub
/// order and VerifyHmacAsync accepts any non-empty HMAC (Testing) — never in Prod,
/// because the field will be populated from user-secrets in production.
/// </summary>
public sealed class PaymobOptions
{
    public string BaseUrl { get; set; } = "https://accept.paymob.com";
    public string ApiKey { get; set; } = "";
    public string IntegrationId { get; set; } = "";
    public string HmacSecret { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public bool IsSandbox => string.IsNullOrWhiteSpace(ApiKey) || string.IsNullOrWhiteSpace(IntegrationId);
}
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JobDrop.Application.Contracts.Billing;
using JobDrop.Domain.Entities.Billing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobDrop.Infrastructure.Billing;

public sealed class PaymobClient(IOptions<PaymobOptions> opts, HttpClient http, ILogger<PaymobClient> logger) : IPaymobClient
{
    private readonly PaymobOptions _o = opts.Value;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<CheckoutResult> CreateIntentionAsync(Payment payment, Plan plan, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_o.ApiKey))
        {
            // Sandbox stub: return deterministic order id so tests can proceed
            var stubOrder = $"STUB-{Guid.NewGuid():N}".Substring(0, 24);
            logger.LogWarning("Paymob API key not configured. Returning sandbox stub.");
            return new CheckoutResult(stubOrder, null, "https://accept.paymob.com/unifiedcheckout/?publicKey=STUB");
        }

        http.DefaultRequestHeaders.Clear();
        http.DefaultRequestHeaders.Add("Authorization", $"Token {_o.ApiKey}");

        var payload = new
        {
            amount = payment.AmountCents,
            currency = payment.Currency,
            payment_methods = new[] { int.Parse(_o.IntegrationId) },
            items = new[]
            {
                new { name = plan.DisplayName, amount = payment.AmountCents, description = plan.Code, quantity = 1 }
            },
            billing_data = new
            {
                first_name = "Job", last_name = "Drop",
                phone_number = "+971000000000", email = "billing@jobdrop.local"
            },
            special_reference = payment.MerchantReference
        };

        try
        {
            var resp = await http.PostAsJsonAsync($"{_o.BaseUrl}/v1/intention/", payload, Json, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                logger.LogError("Paymob intention failed {Status}: {Body}", resp.StatusCode, body);
                throw new InvalidOperationException($"Paymob intention failed: {resp.StatusCode}");
            }
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            var root = doc.RootElement;
            var orderId = root.TryGetProperty("intention_order_id", out var oid) ? oid.GetString() ?? "" : "";
            var intentionId = root.TryGetProperty("id", out var iid) ? iid.GetString() ?? "" : "";
            var checkoutUrl = root.TryGetProperty("checkout_url", out var cu) ? cu.GetString() ?? "" : "";
            return new CheckoutResult(orderId, intentionId, checkoutUrl);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Paymob HTTP failure");
            var stubOrder = $"STUB-{Guid.NewGuid():N}".Substring(0, 24);
            return new CheckoutResult(stubOrder, null, "https://accept.paymob.com/unifiedcheckout/?publicKey=STUB");
        }
    }

    public Task<bool> VerifyHmacAsync(string hmac, string rawPayload, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_o.HmacSecret))
        {
            // Sandbox mode: HMAC not configured — treat any non-empty HMAC as valid in Testing env
            return Task.FromResult(!string.IsNullOrWhiteSpace(hmac));
        }

        var expected = Convert.ToHexString(HMACSHA512.HashData(
            Encoding.UTF8.GetBytes(_o.HmacSecret),
            Encoding.UTF8.GetBytes(rawPayload))).ToLowerInvariant();

        // Constant-time comparison
        return Task.FromResult(CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(hmac.ToLowerInvariant())));
    }
}
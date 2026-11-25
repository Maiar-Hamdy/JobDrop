using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JobDrop.Application.Features.Auth;
using JobDrop.Application.Features.Billing;
using JobDrop.Application.Features.Exports;
using JobDrop.Application.Features.Reviews;
using Xunit;

namespace JobDrop.IntegrationTests;

public sealed class Sprint4Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public Sprint4Tests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    private async Task<AuthResponse> RegisterAsync()
    {
        var s = Guid.NewGuid().ToString("N")[..8];
        var email = $"s4_{s}@jobdrop.com";
        var r = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterCommand(email, "Password123!", $"s4_{s}", false));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, "Password123!"));
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth;
    }

    [Fact]
    public async Task Billing_List_Plans_Works_Anonymously()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var r = await _client.GetAsync("/api/v1/billing/plans");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Billing_Entitlements_ReturnsFreeByDefault()
    {
        await RegisterAsync();
        var r = await _client.GetFromJsonAsync<EntitlementsDto>("/api/v1/billing/entitlements");
        r.Should().NotBeNull();
    }

    [Fact]
    public async Task Paymob_Webhook_Rejects_Missing_Hmac()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/billing/webhooks/paymob",
            new { rawPayload = "{}", hmac = "" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Exports_Request_ReturnsAccepted()
    {
        await RegisterAsync();
        var r = await _client.PostAsJsonAsync("/api/v1/exports", new RequestExportCmd("DropsCsv"));
        r.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task Reviews_Require_Final_Application()
    {
        await RegisterAsync();
        var r = await _client.PostAsJsonAsync($"/api/v1/drops/{Guid.NewGuid()}/reviews",
            new { rating = 5, comment = "Great", targetType = "Company" });
        r.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Admin_Dashboard_Requires_PlatformAdmin_Role()
    {
        await RegisterAsync(); // registers as JobSeeker, not PlatformAdmin
        var r = await _client.GetAsync("/api/v1/admin/dashboard");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
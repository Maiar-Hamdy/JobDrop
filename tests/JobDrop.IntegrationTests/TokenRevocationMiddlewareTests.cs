using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JobDrop.Application.Features.Auth;
using JobDrop.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobDrop.IntegrationTests;

public sealed class TokenRevocationMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TokenRevocationMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturn401_WhenTokenVersionRevoked()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var email = $"revoked_{unique}@jobdrop.com";
        var handle = $"revoked_{unique}";

        var registerPayload = new RegisterCommand(email, "Password123!", handle, false);
        var regResp = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginPayload = new LoginCommand(email, "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);
        var validResponse = await _client.GetAsync("/api/v1/me/profile");
        validResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = db.Users.First(u => u.Email == email);
        user.TokenVersion++;
        await db.SaveChangesAsync();

        var revokedResponse = await _client.GetAsync("/api/v1/me/profile");
        revokedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
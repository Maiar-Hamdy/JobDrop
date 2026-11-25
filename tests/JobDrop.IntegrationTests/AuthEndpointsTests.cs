using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JobDrop.Application.Features.Auth;
using Xunit;

namespace JobDrop.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturn201_WhenValidPayload()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var payload = new RegisterCommand($"test_{unique}@jobdrop.com", "Password123!", $"user_{unique}", false);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().ContainKey("id");
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_WhenValidCredentials()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{unique}@jobdrop.com";
        var handle = $"login_{unique}";
        
        var registerPayload = new RegisterCommand(email, "Password123!", handle, false);
        var regResp = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginPayload = new LoginCommand(email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenInvalidCredentials()
    {
        var loginPayload = new LoginCommand("nonexistent@jobdrop.com", "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
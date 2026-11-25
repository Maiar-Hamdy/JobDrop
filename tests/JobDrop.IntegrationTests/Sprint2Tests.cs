using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JobDrop.Application.Features.Auth;
using JobDrop.Application.Features.Candidates;
using JobDrop.Application.Features.Discovery;
using JobDrop.Application.Features.Drops;
using JobDrop.Application.Features.Insights;
using JobDrop.WebApi.Controllers;
using JobDrop.Domain.Enums;
using Xunit;

namespace JobDrop.IntegrationTests;

public sealed class Sprint2Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public Sprint2Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string email, AuthResponse auth)> RegisterAndLoginAsync()
    {
        var s = Guid.NewGuid().ToString("N")[..8];
        var email = $"s2_{s}@jobdrop.com";
        var reg = new RegisterCommand(email, "Password123!", $"s2_{s}", false);
        var r = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        r.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, "Password123!"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        return (email, auth);
    }

    [Fact]
    public async Task Profile_Get_Returns_Profile()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var r = await _client.GetAsync("/api/v1/me/profile");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var p = await r.Content.ReadFromJsonAsync<ProfileDto>();
        p!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StartingPoint_Add_And_List()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var add = await _client.PostAsJsonAsync("/api/v1/me/starting-points",
            new AddStartingPointCmd("Home", 55.2708, 25.2048, "Primary"));
        add.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await _client.GetFromJsonAsync<List<StartingPointDto>>("/api/v1/me/starting-points");
        list.Should().ContainSingle();
    }

    [Fact]
    public async Task Wallet_Initially_Has_3_Daily_Credits()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var w = await _client.GetFromJsonAsync<WalletDto>("/api/v1/me/wallets");
        w!.DailyLimit.Should().Be(3);
        w.DailyRemaining.Should().Be(3);
    }

    [Fact]
    public async Task DropPreview_When_Job_Does_Not_Exist_Reports_JOB_NOT_FOUND()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var r = await _client.PostAsync($"/api/v1/jobs/{Guid.NewGuid()}/drop-preview", null);
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var p = await r.Content.ReadFromJsonAsync<DropPreviewResult>();
        p!.Eligible.Should().BeFalse();
        p.Reasons.Should().Contain("JOB_NOT_FOUND");
    }

    [Fact]
    public async Task Dispatch_Requires_Idempotency_Key()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var r = await _client.PostAsJsonAsync($"/api/v1/jobs/{Guid.NewGuid()}/drops",
            new DispatchDropBody(null, null));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MyDrops_Summary_Is_Accessible()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var s = await _client.GetFromJsonAsync<MyDropsSummaryDto>("/api/v1/me/drops/summary");
        s.Should().NotBeNull();
        s!.DailyLimit.Should().Be(3);
    }

    [Fact]
    public async Task Discovery_Search_Returns_Empty_When_No_Jobs()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var filter = new DiscoveryFilter(
            null, null, null, null, 55.27, 25.20, TravelMode.Car, 30, 20,
            null, null, null, false, false, SortOrder.MatchDesc);

        var r = await _client.PostAsJsonAsync("/api/v1/discovery/search", filter);
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var ctx = await r.Content.ReadFromJsonAsync<SearchContextDto>();
        ctx.Should().NotBeNull();
    }

    [Fact]
    public async Task Insights_Personal_Returns_Funnel()
    {
        var (_, auth) = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var r = await _client.GetFromJsonAsync<PersonalInsightsDto>("/api/v1/insights/personal?period=all");
        r.Should().NotBeNull();
        r!.SentCount.Should().Be(0);
    }
}
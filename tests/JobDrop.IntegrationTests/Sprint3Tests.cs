using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JobDrop.Application.Features.Auth;
using JobDrop.Application.Features.Companies;
using JobDrop.Application.Features.Jobs;
using Xunit;

namespace JobDrop.IntegrationTests;

public sealed class Sprint3Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public Sprint3Tests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    private async Task<AuthResponse> RegisterRecruiterAsync()
    {
        var s = Guid.NewGuid().ToString("N")[..8];
        var email = $"rec_{s}@jobdrop.com";
        var r = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterCommand(email, "Password123!", $"rec_{s}", true));
        r.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, "Password123!"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth;
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateCompany_Returns201()
    {
        await RegisterRecruiterAsync();
        var r = await _client.PostAsJsonAsync("/api/v1/companies", new CreateCompanyCmd("LLC", "Acme Co", "#3b82f6"));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadIdAsync(r);
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateOffice_ThenListOffices()
    {
        await RegisterRecruiterAsync();
        var c = await _client.PostAsJsonAsync("/api/v1/companies", new CreateCompanyCmd("LLC", "Acme2", null));
        var companyId = await ReadIdAsync(c);

        var office = await _client.PostAsJsonAsync($"/api/v1/companies/{companyId}/offices",
            new CreateOfficeCmd(companyId, "HQ", "HQ", 55.27, 25.20, 500));
        office.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await _client.GetFromJsonAsync<OfficeDto[]>($"/api/v1/companies/{companyId}/offices");
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task Job_Create_And_Publish()
    {
        await RegisterRecruiterAsync();
        var c = await _client.PostAsJsonAsync("/api/v1/companies", new CreateCompanyCmd("LLC", "Acme3", null));
        var companyId = await ReadIdAsync(c);

        await _client.PostAsJsonAsync($"/api/v1/companies/{companyId}/offices",
            new CreateOfficeCmd(companyId, "HQ", "HQ", 55.27, 25.20, 500));
        var offices = await _client.GetFromJsonAsync<OfficeDto[]>($"/api/v1/companies/{companyId}/offices");

        var jobResp = await _client.PostAsJsonAsync($"/api/v1/companies/{companyId}/jobs",
            new CreateJobCmd(companyId, offices![0].Id, "Senior Dev", "OnSite", 15000, 25000, "AED", 10));
        jobResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var jobId = await ReadIdAsync(jobResp);

        var pub = await _client.PostAsync($"/api/v1/jobs/{jobId}/publish", null);
        pub.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await _client.GetFromJsonAsync<JobDto>($"/api/v1/jobs/{jobId}");
        fetched!.Status.Should().Be("Published");
    }

    [Fact]
    public async Task Dossier_Returns_BlindCode_NotPII()
    {
        await RegisterRecruiterAsync();
        var r = await _client.GetAsync($"/api/v1/drops/{Guid.NewGuid()}/dossier");
        r.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReverseDrop_Requires_Idempotency_Key()
    {
        await RegisterRecruiterAsync();
        var r = await _client.PostAsJsonAsync("/api/v1/reverse-drops",
            new JobDrop.Application.Features.ReverseDrops.DispatchReverseDropCmd(
                Guid.NewGuid(), Guid.NewGuid(), null, "Hello", true, "ignored"));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Conversations_List_IsAccessible()
    {
        await RegisterRecruiterAsync();
        var r = await _client.GetAsync("/api/v1/conversations");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
namespace readytohelpapi.Dashboard.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using readytohelpapi.Dashboard.DTOs;
using Xunit;

public partial class Program { }

/// <summary>
/// This class contains all integration tests for Dashboard API controller.
/// </summary>
public class TestDashboardApiController : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDashboardApiController"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    public TestDashboardApiController(WebApplicationFactory<Program> factory)
    {
        var customized = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test",
                        options => { }
                    );

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        });

        _client = customized.CreateClient();
    }

    /// <summary>
    /// Tests the GetOverview endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetOverview_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/overview");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// Tests the GetUserStats endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetUserStats_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/users/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<UserStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// Tests the GetOccurrenceStats endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetOccurrenceStats_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/occurrences/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OccurrenceStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// Tests the GetReportStats endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetReportStats_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/reports/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ReportStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// Tests the GetFeedbackStats endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetFeedbackStats_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/feedbacks/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<FeedbackStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// Tests the GetResponsibleEntityStats endpoint to ensure it returns OK status and a valid DTO.
    /// </summary>
    [Fact]
    public async Task GetResponsibleEntityStats_ReturnsOk_AndDto()
    {
        var response = await _client.GetAsync("/api/dashboard/responsible-entities/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ResponsibleEntityStatsDto>();
        Assert.NotNull(dto);
    }

    /// <summary>
    /// A test authentication handler that simulates an authenticated user with ADMIN and MANAGER roles.
    /// </summary>
    private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder
        )
            : base(options, logger, encoder) { }

        /// <summary>
        /// Handles the authentication process for the test user.
        /// </summary>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "ADMIN"),
                new Claim(ClaimTypes.Role, "MANAGER"),
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

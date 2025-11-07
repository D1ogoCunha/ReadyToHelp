namespace readytohelpapi.Dashboard.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using readytohelpapi.Common.Tests;
using readytohelpapi.Dashboard.DTOs;
using Xunit;

/// <summary>
/// This class contains all integration tests for Dashboard API controller.
/// </summary>
[Trait("Category", "Integration")]
public class TestDashboardApiController : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDashboardApiController"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    public TestDashboardApiController(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
}

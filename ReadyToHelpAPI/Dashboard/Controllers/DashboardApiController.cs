namespace readytohelpapi.Dashboard.Controllers;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Dashboard.DTOs;
using readytohelpapi.Dashboard.Service;

/// <summary>
/// API Controller for dashboard related endpoints.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService dashboardService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardApiController"/> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service.</param>
    public DashboardApiController(IDashboardService dashboardService)
    {
        this.dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets the overview statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardStatsDto>> GetOverview(CancellationToken ct)
    {
        var dto = await dashboardService.GetOverviewAsync(ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets user statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("users/stats")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(CancellationToken ct)
    {
        var dto = await dashboardService.GetUserStatsAsync(ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets occurrence statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("occurrences/stats")]
    public async Task<ActionResult<OccurrenceStatsDto>> GetOccurrenceStats(CancellationToken ct)
    {
        var dto = await dashboardService.GetOccurrenceStatsAsync(ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets report statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("reports/stats")]
    public async Task<ActionResult<ReportStatsDto>> GetReportStats(CancellationToken ct)
    {
        var dto = await dashboardService.GetReportStatsAsync(ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets feedback statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("feedbacks/stats")]
    public async Task<ActionResult<FeedbackStatsDto>> GetFeedbackStats(CancellationToken ct)
    {
        var dto = await dashboardService.GetFeedbackStatsAsync(ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets responsible entity statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("responsible-entities/stats")]
    public async Task<ActionResult<ResponsibleEntityStatsDto>> GetResponsibleEntityStats(
        CancellationToken ct
    )
    {
        var dto = await dashboardService.GetResponsibleEntityStatsAsync(ct);
        return Ok(dto);
    }
}

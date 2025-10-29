namespace readytohelpapi.Dashboard.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Dashboard.Service;
using readytohelpapi.Dashboard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
}
namespace readytohelpapi.Dashboard.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Dashboard.Service;
using readytohelpapi.Dashboard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        this.dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardStatsDto>> GetOverview(CancellationToken ct)
    {
        var dto = await dashboardService.GetOverviewAsync(ct);
        return Ok(dto);
    }
}
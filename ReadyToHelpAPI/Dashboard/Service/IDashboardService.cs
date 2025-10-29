namespace readytohelpapi.Dashboard.Service;

using readytohelpapi.Dashboard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetOverviewAsync(CancellationToken ct = default);
}
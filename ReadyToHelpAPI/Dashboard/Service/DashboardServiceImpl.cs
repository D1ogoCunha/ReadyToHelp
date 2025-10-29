namespace readytohelpapi.Dashboard.Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using readytohelpapi.Common.Data;
using readytohelpapi.Dashboard.DTOs;
using readytohelpapi.Dashboard.Service;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;
using readytohelpapi.User.Models;


public class DashboardServiceImpl : IDashboardService
{
    private readonly AppDbContext _ctx;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DashboardServiceImpl> _logger;

    public DashboardServiceImpl(AppDbContext ctx, IDistributedCache cache, ILogger<DashboardServiceImpl> logger)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DashboardStatsDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var totalOccurrences = await _ctx.Occurrences.CountAsync(ct);
        var totalUsers = await _ctx.Users.CountAsync(ct);
        var totalReports = await _ctx.Reports.CountAsync(ct);

        var inProgress = await _ctx.Occurrences.CountAsync(o => o.Status.ToString() == "IN_PROGRESS", ct);
        var active = await _ctx.Occurrences.CountAsync(o => o.Status.ToString() == "ACTIVE", ct);
        var closed = await _ctx.Occurrences.CountAsync(o => o.Status.ToString() == "CLOSED", ct);

        var avgReportsPerOccurrence = totalOccurrences == 0 ? 0.0 : Math.Round((double)totalReports / totalOccurrences, 2);

        var stats = new DashboardStatsDto
        {
            TotalOccurrences = totalOccurrences,
            TotalUsers = totalUsers,
            InProgressOccurrences = inProgress,
            ActiveOccurrences = active,
            ClosedOccurrences = closed,
            AvgReportsPerOccurrence = avgReportsPerOccurrence,
            AvgResolutionTimeHours = 0.0
        };

        return stats;
    }

    private async Task<double> ComputeAverageResolutionHoursAsync(CancellationToken ct)
    {
        var q = _ctx.Set<Occurrence>().AsQueryable();
        var resolved = await q.Where(o => EF.Property<DateTime?>(o, "ResolvedAt") != null)
                              .Select(o => EF.Property<DateTime?>(o, "ResolvedAt")!.Value - (EF.Property<DateTime?>(o, "CreatedAt") ?? DateTime.MinValue))
                              .ToListAsync(ct);

        if (!resolved.Any()) return 0.0;

        var avg = resolved.Select(ts => ts.TotalHours).Average();
        return Math.Round(avg, 2);
    }
}
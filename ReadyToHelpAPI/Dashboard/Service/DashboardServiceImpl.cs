namespace readytohelpapi.Dashboard.Service;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Dashboard.DTOs;
using readytohelpapi.Occurrence.Models;

/// <summary>
/// Class responsible for dashboard related operations.
/// </summary>
public class DashboardServiceImpl : IDashboardService
{
    private readonly AppDbContext appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardServiceImpl"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    public DashboardServiceImpl(AppDbContext ctx)
    {
        appContext = ctx ?? throw new ArgumentNullException(nameof(ctx));
    }

    /// <summary>
    /// Gets the overview statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="DashboardStatsDto"/> containing the overview statistics.</returns>
    public async Task<DashboardStatsDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var totalOccurrences = await appContext.Occurrences.CountAsync(ct);
        var totalUsers = await appContext.Users.CountAsync(ct);
        var totalReports = await appContext.Reports.CountAsync(ct);
        var totalResponsibleEntities = await appContext.ResponsibleEntities.CountAsync(ct);
        var totalFeedbacks = await appContext.Feedbacks.CountAsync(ct);

        var stats = new DashboardStatsDto
        {
            TotalOccurrences = totalOccurrences,
            TotalUsers = totalUsers,
            TotalReports = totalReports,
            TotalFeedbacks = totalFeedbacks,
            TotalResponsibleEntities = totalResponsibleEntities
        };

        return stats;
    }

    private async Task<double> ComputeAverageResolutionHoursAsync(CancellationToken ct)
    {
        var q = appContext.Set<Occurrence>().AsQueryable();
        var resolved = await q.Where(o => EF.Property<DateTime?>(o, "ResolvedAt") != null)
                              .Select(o => EF.Property<DateTime?>(o, "ResolvedAt")!.Value - (EF.Property<DateTime?>(o, "CreatedAt") ?? DateTime.MinValue))
                              .ToListAsync(ct);

        if (!resolved.Any()) return 0.0;

        var avg = resolved.Select(ts => ts.TotalHours).Average();
        return Math.Round(avg, 2);
    }
}
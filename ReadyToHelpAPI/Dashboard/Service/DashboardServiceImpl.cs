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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default)
    {
        var users = await appContext.Users.AsNoTracking().ToListAsync(ct);
        var reports = await appContext.Reports.AsNoTracking().ToListAsync(ct);
        var feedbacks = await appContext.Feedbacks.AsNoTracking().ToListAsync(ct);

        var total = users.Count;
        var admins = users.Count(u => u.Profile.ToString().Equals("ADMIN", StringComparison.OrdinalIgnoreCase));
        var managers = users.Count(u => u.Profile.ToString().Equals("MANAGER", StringComparison.OrdinalIgnoreCase));
        var citizens = users.Count(u => u.Profile.ToString().Equals("CITIZEN", StringComparison.OrdinalIgnoreCase) || u.Profile.ToString().Equals("USER", StringComparison.OrdinalIgnoreCase));

        var userIdsWithReports = reports.Select(r => r.UserId).Distinct().ToHashSet();
        var userIdsWithFeedbacks = feedbacks.Select(f => f.UserId).Distinct().ToHashSet();

        var usersWithReports = users.Count(u => userIdsWithReports.Contains(u.Id));
        var usersWithFeedbacks = users.Count(u => userIdsWithFeedbacks.Contains(u.Id));
        var usersWithBoth = users.Count(u => userIdsWithReports.Contains(u.Id) && userIdsWithFeedbacks.Contains(u.Id));
        var usersWithoutReportsOrFeedbacks = users.Count(u => !userIdsWithReports.Contains(u.Id) && !userIdsWithFeedbacks.Contains(u.Id));

        var mostActiveUser = users
            .Select(u => new
            {
                UserId = u.Id,
                Name = u.Name,
                Reports = reports.Count(r => r.UserId == u.Id),
                Feedbacks = feedbacks.Count(f => f.UserId == u.Id)
            })
            .OrderByDescending(x => x.Reports + x.Feedbacks)
            .FirstOrDefault();

        return new UserStatsDto
        {
            TotalUsers = total,
            Admins = admins,
            Managers = managers,
            Citizens = citizens,
            UsersWithReports = usersWithReports,
            UsersWithFeedbacks = usersWithFeedbacks,
            UsersWithBoth = usersWithBoth,
            UsersWithoutReportsOrFeedbacks = usersWithoutReportsOrFeedbacks,
            MostActiveUserId = mostActiveUser?.UserId ?? 0,
            MostActiveUserName = mostActiveUser?.Name ?? "",
            MostActiveUserReports = mostActiveUser?.Reports ?? 0,
            MostActiveUserFeedbacks = mostActiveUser?.Feedbacks ?? 0
        };
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
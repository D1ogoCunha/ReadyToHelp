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

    /// <inheritdoc />
    public async Task<OccurrenceStatsDto> GetOccurrenceStatsAsync(CancellationToken ct = default)
    {
        var occurrences = await appContext.Occurrences.AsNoTracking().ToListAsync(ct);

        var total = occurrences.Count;

        var waiting = occurrences.Count(o => o.Status == OccurrenceStatus.WAITING);
        var active = occurrences.Count(o => o.Status == OccurrenceStatus.ACTIVE);
        var inProgress = occurrences.Count(o => o.Status == OccurrenceStatus.IN_PROGRESS);
        var resolved = occurrences.Count(o => o.Status == OccurrenceStatus.RESOLVED);
        var closed = occurrences.Count(o => o.Status == OccurrenceStatus.CLOSED);

        var high = occurrences.Count(o => o.Priority == PriorityLevel.HIGH);
        var medium = occurrences.Count(o => o.Priority == PriorityLevel.MEDIUM);
        var low = occurrences.Count(o => o.Priority == PriorityLevel.LOW);

        var since = DateTime.UtcNow.AddDays(-30);
        var newLast30 = occurrences.Count(o => o.CreationDateTime >= since);

        var byType = occurrences
            .GroupBy(o => o.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var mostReported = occurrences
            .OrderByDescending(o => o.ReportCount)
            .ThenByDescending(o => o.CreationDateTime)
            .FirstOrDefault();

        var avgHours = await ComputeAverageResolutionHoursAsync(ct);

        return new OccurrenceStatsDto
        {
            TotalOccurrences = total,
            Waiting = waiting,
            Active = active,
            InProgress = inProgress,
            Resolved = resolved,
            Closed = closed,
            HighPriority = high,
            MediumPriority = medium,
            LowPriority = low,
            NewOccurrencesLast30Days = newLast30,
            AverageResolutionHours = avgHours,
            MostReportedOccurrenceId = mostReported?.Id ?? 0,
            MostReportedOccurrenceTitle = mostReported?.Title ?? string.Empty,
            MostReports = mostReported?.ReportCount ?? 0,
            ByType = byType
        };
    }

    /// <summary>
    /// Calcula o tempo médio de resolução (em horas) para ocorrências com EndDateTime válido.
    /// Materializa antes de filtrar para evitar problemas de tradução do provider.
    /// </summary>
    private async Task<double> ComputeAverageResolutionHoursAsync(CancellationToken ct)
    {
        var times = await appContext.Occurrences.AsNoTracking()
            .Select(o => new { o.CreationDateTime, o.EndDateTime })
            .ToListAsync(ct);

        var durations = times
            .Where(x => x.EndDateTime != default && x.EndDateTime > x.CreationDateTime)
            .Select(x => (x.EndDateTime - x.CreationDateTime).TotalHours)
            .ToList();

        if (durations.Count == 0) return 0.0;

        return Math.Round(durations.Average(), 2);
    }
}
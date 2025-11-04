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
            TotalResponsibleEntities = totalResponsibleEntities,
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
        var admins = users.Count(u =>
            u.Profile.ToString().Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
        );
        var managers = users.Count(u =>
            u.Profile.ToString().Equals("MANAGER", StringComparison.OrdinalIgnoreCase)
        );
        var citizens = users.Count(u =>
            u.Profile.ToString().Equals("CITIZEN", StringComparison.OrdinalIgnoreCase)
            || u.Profile.ToString().Equals("USER", StringComparison.OrdinalIgnoreCase)
        );

        var userIdsWithReports = reports.Select(r => r.UserId).Distinct().ToHashSet();
        var userIdsWithFeedbacks = feedbacks.Select(f => f.UserId).Distinct().ToHashSet();

        var usersWithReports = users.Count(u => userIdsWithReports.Contains(u.Id));
        var usersWithFeedbacks = users.Count(u => userIdsWithFeedbacks.Contains(u.Id));
        var usersWithBoth = users.Count(u =>
            userIdsWithReports.Contains(u.Id) && userIdsWithFeedbacks.Contains(u.Id)
        );
        var usersWithoutReportsOrFeedbacks = users.Count(u =>
            !userIdsWithReports.Contains(u.Id) && !userIdsWithFeedbacks.Contains(u.Id)
        );

        var mostActiveUser = users
            .Select(u => new
            {
                UserId = u.Id,
                Name = u.Name,
                Reports = reports.Count(r => r.UserId == u.Id),
                Feedbacks = feedbacks.Count(f => f.UserId == u.Id),
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
            MostActiveUserFeedbacks = mostActiveUser?.Feedbacks ?? 0,
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
            ByType = byType,
        };
    }

    /// <inheritdoc />
    public async Task<ReportStatsDto> GetReportStatsAsync(CancellationToken ct = default)
    {
        var reports = await appContext.Reports.AsNoTracking().ToListAsync(ct);

        var now = DateTime.UtcNow;
        var total = reports.Count;

        var last24 = reports.Count(r => (now - r.ReportDateTime).TotalHours <= 24);
        var last7 = reports.Count(r => r.ReportDateTime >= now.AddDays(-7));
        var last30 = reports.Count(r => r.ReportDateTime >= now.AddDays(-30));

        var uniqueReporters = reports.Select(r => r.UserId).Distinct().Count();
        var avgPerDayLast30 = last30 / 30d;

        var byType = reports
            .GroupBy(r => r.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var top = reports
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.UserId)
            .FirstOrDefault();

        string? topName = null;
        if (top != null)
        {
            topName = await appContext
                .Users.AsNoTracking()
                .Where(u => u.Id == top.UserId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync(ct);
        }

        var firstDate = total > 0 ? reports.Min(r => r.ReportDateTime) : (DateTime?)null;
        var lastDate = total > 0 ? reports.Max(r => r.ReportDateTime) : (DateTime?)null;

        return new ReportStatsDto
        {
            TotalReports = total,
            NewReportsLast24Hours = last24,
            NewReportsLast7Days = last7,
            NewReportsLast30Days = last30,
            UniqueReporters = uniqueReporters,
            AverageReportsPerDayLast30 = Math.Round(avgPerDayLast30, 2),
            ReportsByType = byType,
            TopReporterUserId = top?.UserId ?? 0,
            TopReporterUserName = topName ?? string.Empty,
            TopReporterReportCount = top?.Count ?? 0,
            FirstReportDate = firstDate,
            LastReportDate = lastDate,
        };
    }

    /// <inheritdoc />
    public async Task<FeedbackStatsDto> GetFeedbackStatsAsync(CancellationToken ct = default)
    {
        var feedbacks = await appContext.Feedbacks.AsNoTracking().ToListAsync(ct);

        var now = DateTime.UtcNow;
        var total = feedbacks.Count;

        var last24 = feedbacks.Count(fb => (now - fb.FeedbackDateTime).TotalHours <= 24);
        var last7 = feedbacks.Count(fb => fb.FeedbackDateTime >= now.AddDays(-7));
        var last30 = feedbacks.Count(fb => fb.FeedbackDateTime >= now.AddDays(-30));

        var uniqueUsers = feedbacks.Select(fb => fb.UserId).Distinct().Count();

        var confirmed = feedbacks.Count(fb => fb.IsConfirmed);
        var notConfirmed = total - confirmed;
        var confirmationRate = total == 0 ? 0.0 : Math.Round((double)confirmed / total * 100.0, 2);

        var avgPerDayLast30 = Math.Round(last30 / 30d, 2);

        var top = feedbacks
            .GroupBy(fb => fb.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.UserId)
            .FirstOrDefault();

        string? topName = null;
        if (top != null)
        {
            topName = await appContext
                .Users.AsNoTracking()
                .Where(u => u.Id == top.UserId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync(ct);
        }

        var firstDate = total > 0 ? feedbacks.Min(fb => fb.FeedbackDateTime) : (DateTime?)null;
        var lastDate = total > 0 ? feedbacks.Max(fb => fb.FeedbackDateTime) : (DateTime?)null;

        return new FeedbackStatsDto
        {
            TotalFeedbacks = total,
            NewFeedbacksLast24Hours = last24,
            NewFeedbacksLast7Days = last7,
            NewFeedbacksLast30Days = last30,
            UniqueUsers = uniqueUsers,
            ConfirmedCount = confirmed,
            NotConfirmedCount = notConfirmed,
            ConfirmationRate = confirmationRate,
            AverageFeedbacksPerDayLast30 = avgPerDayLast30,
            TopFeedbackUserId = top?.UserId ?? 0,
            TopFeedbackUserName = topName ?? string.Empty,
            TopFeedbackUserCount = top?.Count ?? 0,
            FirstFeedbackDate = firstDate,
            LastFeedbackDate = lastDate,
        };
    }

    /// <inheritdoc />
    public async Task<ResponsibleEntityStatsDto> GetResponsibleEntityStatsAsync(
        CancellationToken ct = default
    )
    {
        var entities = await appContext.ResponsibleEntities.AsNoTracking().ToListAsync(ct);
        var occurrences = await appContext.Occurrences.AsNoTracking().ToListAsync(ct);

        var totalEntities = entities.Count;

        var byType = entities
            .GroupBy(e => e.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var assignedOccs = occurrences.Where(o => o.ResponsibleEntityId > 0).ToList();
        var entityIdsWithOcc = assignedOccs
            .Select(o => o.ResponsibleEntityId)
            .Distinct()
            .ToHashSet();

        var entitiesWithAssigned = entityIdsWithOcc.Count;
        var entitiesWithoutAssigned = totalEntities - entitiesWithAssigned;

        var totalAssignedOccurrences = assignedOccs.Count;
        var averageOccurrencesPerEntity =
            totalEntities == 0
                ? 0.0
                : Math.Round(totalAssignedOccurrences / (double)totalEntities, 2);

        var activeStatuses = new[]
        {
            OccurrenceStatus.WAITING,
            OccurrenceStatus.ACTIVE,
            OccurrenceStatus.IN_PROGRESS,
        };
        var activeOccurrences = assignedOccs.Count(o => activeStatuses.Contains(o.Status));

        var occByStatus = occurrences
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var withContact = entities.Count(e => !string.IsNullOrWhiteSpace(e.Email));
        var withoutContact = totalEntities - withContact;

        var top = assignedOccs
            .GroupBy(o => o.ResponsibleEntityId)
            .Select(g => new { EntityId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.EntityId)
            .FirstOrDefault();

        var topName =
            top == null
                ? string.Empty
                : entities.FirstOrDefault(e => e.Id == top.EntityId)?.Name ?? string.Empty;

        return new ResponsibleEntityStatsDto
        {
            TotalResponsibleEntities = totalEntities,
            ByType = byType,
            EntitiesWithAssignedOccurrences = entitiesWithAssigned,
            EntitiesWithoutAssignedOccurrences = entitiesWithoutAssigned,
            TotalAssignedOccurrences = totalAssignedOccurrences,
            AverageOccurrencesPerEntity = averageOccurrencesPerEntity,
            ActiveOccurrences = activeOccurrences,
            OccurrencesByStatus = occByStatus,
            EntitiesWithContactInfo = withContact,
            EntitiesWithoutContactInfo = withoutContact,
            TopEntityByOccurrencesId = top?.EntityId ?? 0,
            TopEntityByOccurrencesName = topName,
            TopEntityByOccurrencesCount = top?.Count ?? 0,
        };
    }

    /// <summary>
    /// Calcula o tempo médio de resolução (em horas) para ocorrências com EndDateTime válido.
    /// Materializa antes de filtrar para evitar problemas de tradução do provider.
    /// </summary>
    private async Task<double> ComputeAverageResolutionHoursAsync(CancellationToken ct)
    {
        var times = await appContext
            .Occurrences.AsNoTracking()
            .Select(o => new { o.CreationDateTime, o.EndDateTime })
            .ToListAsync(ct);

        var durations = times
            .Where(x => x.EndDateTime != default && x.EndDateTime > x.CreationDateTime)
            .Select(x => (x.EndDateTime - x.CreationDateTime).TotalHours)
            .ToList();

        if (durations.Count == 0)
            return 0.0;

        return Math.Round(durations.Average(), 2);
    }
}

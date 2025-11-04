namespace readytohelpapi.Dashboard.Service;

using System.Threading;
using System.Threading.Tasks;
using readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Service interface for dashboard-related operations.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets the overview statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="DashboardStatsDto"/> containing the overview statistics.</returns>
    Task<DashboardStatsDto> GetOverviewAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the user statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="UserStatsDto"/> containing the user statistics.</returns>
    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the occurrence statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="OccurrenceStatsDto"/> containing the occurrence statistics.</returns>
    Task<OccurrenceStatsDto> GetOccurrenceStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the report statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A ReportStatsDto with aggregated report metrics.</returns>
    Task<ReportStatsDto> GetReportStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the feedback statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A FeedbackStatsDto with aggregated feedback metrics.</returns>
    Task<FeedbackStatsDto> GetFeedbackStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the responsible entity statistics for the dashboard.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A ResponsibleEntityStatsDto with aggregated metrics.</returns>
    Task<ResponsibleEntityStatsDto> GetResponsibleEntityStatsAsync(CancellationToken ct = default);
}

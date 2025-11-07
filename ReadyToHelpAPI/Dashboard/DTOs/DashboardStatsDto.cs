namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Aggregated statistics for the dashboard overview.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Gets or sets the total number of occurrences.
    /// </summary>
    public int TotalOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the total number of reports.
    /// </summary>
    public int TotalReports { get; set; }

    /// <summary>
    /// Gets or sets the total number of feedbacks.
    /// </summary>
    public int TotalFeedbacks { get; set; }

    /// <summary>
    /// Gets or sets the total number of responsible entities.
    /// </summary>
    public int TotalResponsibleEntities { get; set; }
}

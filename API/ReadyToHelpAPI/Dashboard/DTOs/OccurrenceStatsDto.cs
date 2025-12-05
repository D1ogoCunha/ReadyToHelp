namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for occurrences.
/// </summary>
public class OccurrenceStatsDto
{
    /// <summary>
    /// Gets or sets the total number of occurrences.
    /// </summary>
    public int TotalOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences in the waiting state.
    /// </summary>
    public int Waiting { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences in the active state.
    /// </summary>
    public int Active { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences in the in-progress state.
    /// </summary>
    public int InProgress { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences in the resolved state.
    /// </summary>
    public int Resolved { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences in the closed state.
    /// </summary>
    public int Closed { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences with high priority.
    /// </summary>
    public int HighPriority { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences with medium priority.
    /// </summary>
    public int MediumPriority { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences with low priority.
    /// </summary>
    public int LowPriority { get; set; }

    /// <summary>
    /// Gets or sets the number of new occurrences in the last 30 days.
    /// </summary>
    public int NewOccurrencesLast30Days { get; set; }

    /// <summary>
    /// Gets or sets the average resolution time in hours.
    /// </summary>
    public double AverageResolutionHours { get; set; }

    /// <summary>
    /// Gets or sets the occurrence with the most reports.
    /// </summary>
    public int MostReportedOccurrenceId { get; set; }

    /// <summary>
    /// Gets or sets the title of the occurrence with the most reports.
    /// </summary>
    public string? MostReportedOccurrenceTitle { get; set; }

    /// <summary>
    /// Gets or sets the number of reports for the occurrence with the most reports.
    /// </summary>
    public int MostReports { get; set; }

    /// <summary>
    /// Gets or sets the breakdown of occurrences by type.
    /// </summary>
    public Dictionary<string, int> ByType { get; set; } = new();
}

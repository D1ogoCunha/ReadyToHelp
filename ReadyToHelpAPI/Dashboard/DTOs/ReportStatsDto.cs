namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for reports.
/// </summary>
public class ReportStatsDto
{
    /// <summary>
    /// Gets or sets the total number of reports.
    /// </summary>
    public int TotalReports { get; set; }

    /// <summary>
    /// Gets or sets the number of new reports in the last 24 hours.
    /// </summary>
    public int NewReportsLast24Hours { get; set; }

    /// <summary>
    /// Gets or sets the number of new reports in the last 7 days.
    /// </summary>
    public int NewReportsLast7Days { get; set; }

    /// <summary>
    /// Gets or sets the number of new reports in the last 30 days.
    /// </summary>
    public int NewReportsLast30Days { get; set; }

    /// <summary>
    /// Gets or sets the number of unique reporters.
    /// </summary>
    public int UniqueReporters { get; set; }

    /// <summary>
    /// Gets or sets the average number of reports per day in the last 30 days.
    /// </summary>
    public double AverageReportsPerDayLast30 { get; set; }

    /// <summary>
    /// Gets or sets the number of reports by type.
    /// </summary>
    public Dictionary<string, int> ReportsByType { get; set; } = new();

    /// <summary>
    /// Gets or sets the user ID of the top reporter.
    /// </summary>
    public int TopReporterUserId { get; set; }

    /// <summary>
    /// Gets or sets the user name of the top reporter.
    /// </summary>
    public string? TopReporterUserName { get; set; }

    /// <summary>
    /// Gets or sets the number of reports by the top reporter.
    /// </summary>
    public int TopReporterReportCount { get; set; }

    /// <summary>
    /// Gets or sets the date of the first report.
    /// </summary>
    public DateTime? FirstReportDate { get; set; }

    /// <summary>
    /// Gets or sets the date of the last report.
    /// </summary>
    public DateTime? LastReportDate { get; set; }
}

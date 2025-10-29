namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for reports.
/// </summary>
public class ReportStatsDto
{
    public int TotalReports { get; set; }

    public int NewReportsLast24Hours { get; set; }
    public int NewReportsLast7Days { get; set; }
    public int NewReportsLast30Days { get; set; }

    public int UniqueReporters { get; set; }
    public double AverageReportsPerDayLast30 { get; set; }

    public Dictionary<string, int> ReportsByType { get; set; } = new();

    public int TopReporterUserId { get; set; }
    public string? TopReporterUserName { get; set; }
    public int TopReporterReportCount { get; set; }

    public DateTime? FirstReportDate { get; set; }
    public DateTime? LastReportDate { get; set; }
}
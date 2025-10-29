namespace readytohelpapi.Dashboard.DTOs;

public class OccurrenceStatsDto
{
    public int TotalOccurrences { get; set; }

    public int Waiting { get; set; }
    public int Active { get; set; }
    public int InProgress { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }

    public int HighPriority { get; set; }
    public int MediumPriority { get; set; }
    public int LowPriority { get; set; }

    public int NewOccurrencesLast30Days { get; set; }
    public double AverageResolutionHours { get; set; }

    public int MostReportedOccurrenceId { get; set; }
    public string? MostReportedOccurrenceTitle { get; set; }
    public int MostReports { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}
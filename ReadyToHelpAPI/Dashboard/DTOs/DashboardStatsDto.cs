namespace readytohelpapi.Dashboard.DTOs;

public class DashboardStatsDto
{
    public int TotalOccurrences { get; set; }
    public int ActiveOccurrences { get; set; }
    public int InProgressOccurrences { get; set; }
    public int ClosedOccurrences { get; set; }
    public int TotalUsers { get; set; }
    public double AvgReportsPerOccurrence { get; set; }
    public double AvgResolutionTimeHours { get; set; }
}
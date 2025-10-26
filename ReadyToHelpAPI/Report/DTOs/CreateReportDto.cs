namespace readytohelpapi.Report.DTOs;

using readytohelpapi.Occurrence.Models;

/// <summary>
/// Data transfer object for creating a report.
/// </summary>
public class CreateReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OccurrenceType Type { get; set; }
    public int UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
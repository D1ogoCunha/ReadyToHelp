namespace readytohelpapi.Report.DTOs;

using readytohelpapi.Occurrence.Models;

/// <summary>
/// Data transfer object for report responses.
/// </summary>
public class ReportResponseDto
{
    public int ReportId { get; set; }
    public int OccurrenceId { get; set; }
    public OccurrenceStatus OccurrenceStatus { get; set; }
    public ResponsibleEntityContactDto? ResponsibleEntity { get; set; }
}

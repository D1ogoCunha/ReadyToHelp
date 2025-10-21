namespace readytohelpapi.Report.DTOs;

using readytohelpapi.Occurrence.Models;

public class ReportResponseDto
{
    public int ReportId { get; set; }
    public int OccurrenceId { get; set; }
    public OccurrenceStatus OccurrenceStatus { get; set; }
    public ResponsibleEntityContactDto? ResponsibleEntity { get; set; }
}

public class ResponsibleEntityContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ContactPhone { get; set; }
}
namespace readytohelpapi.Occurrence.Models;

/// <summary>
///  Data Transfer Object for detailed occurrence information.
/// </summary>
public class OccurrenceDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OccurrenceType Type { get; set; }
    public OccurrenceStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public int? ResponsibleEntityId { get; set; }
    public int ReportCount { get; set; }
}
namespace readytohelpapi.Occurrence.DTOs;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;

/// <summary>
/// Data transfer object for creating an Occurrence to reduce constructor parameters.
/// </summary>
public class OccurrenceCreateDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OccurrenceType Type { get; set; }
    public OccurrenceStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public double ProximityRadius { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int ReportCount { get; set; }
    public int? ReportId { get; set; }
    public int ResponsibleEntityId { get; set; }
    public GeoPoint Location { get; set; } = new GeoPoint();
}

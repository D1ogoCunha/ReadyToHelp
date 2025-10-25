namespace readytohelpapi.Occurrence.Models;

/// <summary>
///  Data Transfer Object for occurrence map information.
/// </summary>
public class OccurrenceMapDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public OccurrenceType Type { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public OccurrenceStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
}
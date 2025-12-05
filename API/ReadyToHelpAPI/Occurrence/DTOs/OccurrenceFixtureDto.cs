using readytohelpapi.Occurrence.Models;

namespace readytohelpapi.Occurrence.DTOs;

public class OccurrenceFixtureDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = "Default title";
    public string Description { get; set; } = "Default description";
    public OccurrenceType Type { get; set; } = OccurrenceType.FOREST_FIRE;
    public OccurrenceStatus Status { get; set; } = OccurrenceStatus.ACTIVE;
    public PriorityLevel Priority { get; set; } = PriorityLevel.MEDIUM;
    public double ProximityRadius { get; set; } = 100;
    public DateTime? EndDateTime { get; set; }
    public int ReportCount { get; set; } = 0;
    public int ReportId { get; set; } = 0;
    public int ResponsibleEntityId { get; set; } = 0;
    public double Latitude { get; set; } = 0;
    public double Longitude { get; set; } = 0;
}

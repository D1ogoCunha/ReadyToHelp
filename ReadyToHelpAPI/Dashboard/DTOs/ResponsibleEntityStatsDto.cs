namespace readytohelpapi.Dashboard.DTOs;

public class ResponsibleEntityStatsDto
{
    public int TotalResponsibleEntities { get; set; }

    public Dictionary<string, int> ByType { get; set; } = new();

    public int EntitiesWithAssignedOccurrences { get; set; }
    public int EntitiesWithoutAssignedOccurrences { get; set; }

    public int TotalAssignedOccurrences { get; set; }
    public double AverageOccurrencesPerEntity { get; set; }

    public int ActiveOccurrences { get; set; }
    public Dictionary<string, int> OccurrencesByStatus { get; set; } = new();

    public int EntitiesWithContactInfo { get; set; }
    public int EntitiesWithoutContactInfo { get; set; }

    public int TopEntityByOccurrencesId { get; set; }
    public string? TopEntityByOccurrencesName { get; set; }
    public int TopEntityByOccurrencesCount { get; set; }
}
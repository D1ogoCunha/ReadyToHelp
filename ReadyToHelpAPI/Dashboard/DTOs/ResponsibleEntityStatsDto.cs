namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for responsible entities.
/// </summary>
public class ResponsibleEntityStatsDto
{
    /// <summary>
    /// Gets or sets the total number of responsible entities.
    /// </summary>
    public int TotalResponsibleEntities { get; set; }

    /// <summary>
    /// Gets or sets the number of responsible entities by type.
    /// </summary>
    public Dictionary<string, int> ByType { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of entities with assigned occurrences.
    /// </summary>
    public int EntitiesWithAssignedOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the number of entities without assigned occurrences.
    /// </summary>
    public int EntitiesWithoutAssignedOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the total number of assigned occurrences.
    /// </summary>
    public int TotalAssignedOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the average number of occurrences per entity.
    /// </summary>
    public double AverageOccurrencesPerEntity { get; set; }

    /// <summary>
    /// Gets or sets the number of active occurrences.
    /// </summary>
    public int ActiveOccurrences { get; set; }

    /// <summary>
    /// Gets or sets the occurrences by status.
    /// </summary>
    public Dictionary<string, int> OccurrencesByStatus { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of entities with contact information.
    /// </summary>
    public int EntitiesWithContactInfo { get; set; }

    /// <summary>
    /// Gets or sets the number of entities without contact information.
    /// </summary>
    public int EntitiesWithoutContactInfo { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the top entity by occurrences.
    /// </summary>
    public int TopEntityByOccurrencesId { get; set; }

    /// <summary>
    /// Gets or sets the name of the top entity by occurrences.
    /// </summary>
    public string? TopEntityByOccurrencesName { get; set; }

    /// <summary>
    /// Gets or sets the count of occurrences for the top entity.
    /// </summary>
    public int TopEntityByOccurrencesCount { get; set; }
}
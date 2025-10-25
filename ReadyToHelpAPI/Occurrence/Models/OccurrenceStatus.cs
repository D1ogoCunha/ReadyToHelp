namespace readytohelpapi.Occurrence.Models;

/// <summary>
/// Defines the status of an occurrence.
/// </summary>
public enum OccurrenceStatus
{
    WAITING,
    ACTIVE,
    IN_PROGRESS,
    RESOLVED,
    CLOSED
}
namespace readytohelpapi.ResponsibleEntity.Services;

using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;

/// <summary>
/// Defines the contract for responsible entity services.
/// </summary>
public interface IResponsibleEntityService
{
    /// <summary>
    /// Finds the responsible entity for a given occurrence type and location.
    /// </summary>
    /// <param name="occurrenceType">The type of occurrence.</param>
    /// <param name="latitude">The latitude of the occurrence.</param>
    /// <param name="longitude">The longitude of the occurrence.</param>
    /// <returns>The responsible entity or null if none found.</returns>
    ResponsibleEntity? FindResponsibleEntity(OccurrenceType occurrenceType, double latitude, double longitude);
}
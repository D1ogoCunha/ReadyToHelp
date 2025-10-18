namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Models;
using System;
using System.Collections.Generic;

/// <summary>
///    Implements the occurrence service operations.
/// </summary>
public class OccurrenceServiceImpl : IOccurrenceService
{
    private readonly IOccurrenceRepository occurrenceRepository;

    /// <summary>
    ///   Initializes a new instance of the <see cref="OccurrenceServiceImpl"/> class.
    /// </summary>
    /// <param name="occurrenceRepository">The occurrence repository instance.</param>
    public OccurrenceServiceImpl(IOccurrenceRepository occurrenceRepository)
    {
        this.occurrenceRepository = occurrenceRepository;
    }

    /// <summary>
    ///  Creates an occurrence.
    /// </summary>
    public Occurrence Create(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));

        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException("Occurrence title cannot be null or empty", nameof(occurrence.Title));

        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException("Occurrence description cannot be null or empty", nameof(occurrence.Description));

        if (!Enum.IsDefined(typeof(OccurrenceType), occurrence.Type))
            throw new ArgumentOutOfRangeException(nameof(occurrence.Type), "Invalid occurrence type");

        if (!Enum.IsDefined(typeof(PriorityLevel), occurrence.Priority))
            throw new ArgumentOutOfRangeException(nameof(occurrence.Priority), "Invalid priority level");

        if (occurrence.ProximityRadius <= 0)
            throw new ArgumentException("Proximity radius must be greater than zero.", nameof(occurrence.ProximityRadius));

        if (occurrence.ReportCount < 0)
            throw new ArgumentException("ReportCount cannot be negative.", nameof(occurrence.ReportCount));

        if (occurrence.ReportId < 0)
            throw new ArgumentException("ReportId cannot be negative.", nameof(occurrence.ReportId));

        if (occurrence.ResponsibleEntityId < 0)
            throw new ArgumentException("ResponsibleEntityId cannot be negative.", nameof(occurrence.ResponsibleEntityId));

        occurrence.CreationDateTime = DateTime.UtcNow;
        if (occurrence.EndDateTime != default && occurrence.EndDateTime <= occurrence.CreationDateTime)
            throw new ArgumentException("EndDateTime must be later than CreationDateTime.", nameof(occurrence.EndDateTime));

        try
        {
            return this.occurrenceRepository.Create(occurrence);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to create an occurrence.", e);
        }
    }

    /// <summary>
    /// Updates an occurrence.
    /// </summary>
    public Occurrence Update(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));

        if (occurrence.Id <= 0)
            throw new ArgumentException("Invalid occurrence ID.", nameof(occurrence.Id));

        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException("Occurrence title cannot be null or empty.", nameof(occurrence.Title));

        if (string.IsNullOrWhiteSpace(occurrence.Description))
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));

        if (occurrence.Id <= 0)
            throw new ArgumentException("Invalid occurrence ID.", nameof(occurrence.Id));

        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException("Occurrence title cannot be null or empty.", nameof(occurrence.Title));

        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException("Occurrence description cannot be null or empty.", nameof(occurrence.Description));

        if (!Enum.IsDefined(typeof(OccurrenceType), occurrence.Type))
            throw new ArgumentOutOfRangeException(nameof(occurrence.Type), "Invalid occurrence type");

        if (!Enum.IsDefined(typeof(PriorityLevel), occurrence.Priority))
            throw new ArgumentOutOfRangeException(nameof(occurrence.Priority), "Invalid priority level");

        if (occurrence.ProximityRadius <= 0)
            throw new ArgumentException("Proximity radius must be greater than zero.", nameof(occurrence.ProximityRadius));

        if (occurrence.ReportCount < 0)
            throw new ArgumentException("ReportCount cannot be negative.", nameof(occurrence.ReportCount));

        if (occurrence.ReportId < 0)
            throw new ArgumentException("ReportId cannot be negative.", nameof(occurrence.ReportId));

        if (occurrence.ResponsibleEntityId < 0)
            throw new ArgumentException("ResponsibleEntityId cannot be negative.", nameof(occurrence.ResponsibleEntityId));

        if (occurrence.EndDateTime != default && occurrence.EndDateTime <= occurrence.CreationDateTime)
            throw new ArgumentException("EndDateTime must be later than CreationDateTime.", nameof(occurrence.EndDateTime));

        if (occurrence.Location is null)
            throw new ArgumentException("Occurrence location is required.", nameof(occurrence.Location));

        if (occurrence.Location.Latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(occurrence.Location.Latitude), "Latitude must be between -90 and 90.");
        
        if (occurrence.Location.Longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(occurrence.Location.Longitude), "Longitude must be between -180 and 180.");

        try
        {
            var updatedOccurrence = this.occurrenceRepository.Update(occurrence);
            if (updatedOccurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {occurrence.Id} not found.");

            return updatedOccurrence;
        }
        catch (KeyNotFoundException) { throw; }
        catch (ArgumentException) { throw; }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while updating the occurrence.", e);
        }
    }

    /// <summary>
    /// Deletes an occurrence.
    /// </summary>
    public Occurrence Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid occurrence ID.", nameof(id));

        try
        {
            var deletedOccurrence = this.occurrenceRepository.Delete(id);
            if (deletedOccurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {id} not found.");

            return deletedOccurrence;
        }
        catch (KeyNotFoundException) { throw; }
        catch (ArgumentException) { throw; }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while deleting the occurrence.", e);
        }
    }

    /// <summary>
    ///   Retrieves an occurrence by ID.
    /// </summary>
    public Occurrence GetOccurrenceById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid occurrence id.");

        var occurrence = this.occurrenceRepository.GetOccurrenceById(id);
        if (occurrence == null)
            throw new KeyNotFoundException($"Occurrence with id {id} not found.");

        return occurrence;
    }

    /// <summary>
    ///  Retrieves occurrences by title.
    /// </summary>
    public List<Occurrence> GetOccurrenceByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        return this.occurrenceRepository.GetOccurrenceByTitle(title);
    }

    /// <summary>
    ///   Retrieves a paginated, filtered, and sorted list of occurrences.
    /// </summary>
    public List<Occurrence> GetAllOccurrences(int pageNumber, int pageSize, string sortBy, string sortOrder, string filter)
    {
        if (string.IsNullOrEmpty(sortBy))
            throw new ArgumentException("Sort field cannot be null or empty.", nameof(sortBy));

        if (sortOrder != "asc" && sortOrder != "desc")
            throw new ArgumentException("Sort order must be 'asc' or 'desc'.", nameof(sortOrder));

        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000.", nameof(pageSize));

        try
        {
            var occurrences = this.occurrenceRepository.GetAllOccurrences(pageNumber, pageSize, sortBy, sortOrder, filter);
            return occurrences ?? new List<Occurrence>();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while retrieving occurrences.", e);
        }
    }

    /// <summary>
    ///   Retrieves occurrences by type.
    /// </summary>
    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return this.occurrenceRepository.GetOccurrencesByType(type);
    }

    /// <summary>
    ///  Retrieves all active occurrences.
    /// </summary>
    public List<Occurrence> GetAllActiveOccurrences()
    {
        return this.occurrenceRepository.GetOccurrencesByStatus(OccurrenceStatus.ACTIVE);
    }

    /// <summary>
    ///  Retrieves occurrences by priority level.
    /// </summary>
    public List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority)
    {
        return this.occurrenceRepository.GetOccurrencesByPriority(priority);
    }
}
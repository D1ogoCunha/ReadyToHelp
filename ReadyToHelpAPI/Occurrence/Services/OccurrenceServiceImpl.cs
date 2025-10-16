namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Models;
using System;
using System.Collections.Generic;

public class OccurrenceServiceImpl : IOccurrenceService
{
    private readonly IOccurrenceRepository occurrenceRepository;

    public OccurrenceServiceImpl(IOccurrenceRepository occurrenceRepository)
    {
        this.occurrenceRepository = occurrenceRepository;
    }

    public Occurrence Create(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence), "Occurrence object is null");

        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException("Occurrence title cannot be null or empty", nameof(occurrence.Title));

        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException("Occurrence description cannot be null or empty", nameof(occurrence.Description));

        try
        {
            return this.occurrenceRepository.Create(occurrence);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to create an occurrence.", e);
        }
    }

    public Occurrence GetOccurrenceById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid occurrence id.");

        var occurrence = this.occurrenceRepository.GetOccurrenceById(id);
        if (occurrence == null)
            throw new KeyNotFoundException($"Occurrence with id {id} not found.");

        return occurrence;
    }

    public List<Occurrence> GetOccurrenceByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        return this.occurrenceRepository.GetOccurrenceByTitle(title);
    }

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

    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return this.occurrenceRepository.GetOccurrencesByType(type);
    }

    public List<Occurrence> GetAllActiveOccurrences()
    {
        return this.occurrenceRepository.GetOccurrencesByStatus(OccurrenceStatus.ACTIVE);
    }

    public List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority)
    {
        return this.occurrenceRepository.GetOccurrencesByPriority(priority);
    }

}
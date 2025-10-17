namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Data;
using readytohelpapi.Occurrence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///    Implements the occurrence repository for managing occurrence data.
/// </summary>
public class OccurrenceRepository : IOccurrenceRepository
{
    private readonly OccurrenceContext occurrenceContext;

    /// <summary>
    ///    Initializes a new instance of the <see cref="OccurrenceRepository"/> class.
    /// </summary>
    /// <param name="context">The occurrence database context.</param>
    public OccurrenceRepository(OccurrenceContext context)
    {
        occurrenceContext = context;
    }

    /// <summary>
    ///   Creates an occurrence in the repository.
    /// </summary>
    /// <param name="occurrence">The occurrence object to be created.</param>
    public Occurrence Create(Occurrence occurrence)
    {
        if (occurrence == null) throw new ArgumentNullException(nameof(occurrence));
        try
        {
            var created = occurrenceContext.Occurrences.Add(occurrence).Entity;
            occurrenceContext.SaveChanges();
            return created;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to create occurrence", ex);
        }
    }

    /// <summary>
    ///   Retrieves an occurrence by ID.
    /// </summary>
    /// <param name="id">The occurrence ID.</param>
    public Occurrence? GetOccurrenceById(int id)
    {
        if (id <= 0) return null;
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    ///   Retrieves occurrences by partial or full title.
    /// </summary>
    /// <param name="title">The title to search for.</param>
    /// <returns>A list of occurrences that match the title.</returns>
    public List<Occurrence> GetOccurrenceByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new List<Occurrence>();

        var pattern = $"%{title.Trim()}%";
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => EF.Functions.ILike(o.Title, pattern))
            .ToList();
    }

    /// <summary>
    ///   Retrieves a paginated, filtered, and sorted list of occurrences.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field by which to sort the results.</param
    /// <param name="sortOrder">The sort order, either "asc" or "desc".</param>
    /// <param name="filter">The string to filter the occurrence data.</param>  
    /// <returns>A paginated, sorted, and filtered list of occurrences.</returns>
    public List<Occurrence> GetAllOccurrences(int pageNumber, int pageSize, string sortBy, string sortOrder, string filter)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var query = occurrenceContext.Occurrences.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var trimmed = filter.Trim();
            var pattern = $"%{trimmed}%";
            query = query.Where(o =>
                EF.Functions.ILike(o.Title ?? string.Empty, pattern) ||
                EF.Functions.ILike(o.Description ?? string.Empty, pattern));
        }

        var asc = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sortBy?.ToLowerInvariant())
        {
            case "title":
                query = asc ? query.OrderBy(o => o.Title) : query.OrderByDescending(o => o.Title);
                break;
            case "priority":
                query = asc ? query.OrderBy(o => o.Priority) : query.OrderByDescending(o => o.Priority);
                break;
            case "status":
                query = asc ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status);
                break;
            default:
                query = asc ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id);
                break;
        }

        var skip = (pageNumber - 1) * pageSize;
        return query.Skip(skip).Take(pageSize).ToList();
    }

    /// <summary>
    ///    Retrieves all occurrences by type.
    /// </summary>
    /// <param name="type">The type of the occurrence.</param>
    /// <returns>A list of occurrences of the specified type.</returns>
    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Type == type)
            .ToList();
    }

    /// <summary>
    ///   Retrieves all occurrences by the specified status.
    /// </summary>
    /// <param name="status">The status of the occurrences to retrieve.</param>
    /// <returns>A list of occurrences with the specified status.</returns>
    public List<Occurrence> GetOccurrencesByStatus(OccurrenceStatus status)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Status == status)
            .ToList();
    }

    /// <summary>
    ///   Retrieves all occurrences by the specified priority level.
    /// </summary>
    /// <param name="priority">The priority level of the occurrence.</param>
    /// <returns>A list of occurrences of the specified priority level.</returns>
    public List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Priority == priority)
            .ToList();
    }

    /// <summary>
    ///   Retrieves all active occurrences.
    /// </summary>
    /// <returns>A list of active occurrences.</returns>
    public List<Occurrence> GetAllActiveOccurrences()
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Status == OccurrenceStatus.ACTIVE)
            .ToList();
    }

    /// <summary>
    ///     Retrieves an occurrence by reportId.
    /// </summary>
    /// <param name="reportId">The report identifier.</param>
    /// <returns>The occurrence with the specified reportId or null.</returns>
    public Occurrence? GetByReportId(int reportId)
    {
        if (reportId <= 0) return null;
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .FirstOrDefault(o => o.ReportId == reportId);
    }
}
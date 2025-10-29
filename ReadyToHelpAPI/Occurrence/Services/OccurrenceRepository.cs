namespace readytohelpapi.Occurrence.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;

/// <summary>
///    Implements the occurrence repository for managing occurrence data.
/// </summary>
public class OccurrenceRepository : IOccurrenceRepository
{
    private readonly AppDbContext occurrenceContext;

    /// <summary>
    ///    Initializes a new instance of the <see cref="OccurrenceRepository"/> class.
    /// </summary>
    /// <param name="context">The occurrence database context.</param>
    public OccurrenceRepository(AppDbContext context)
    {
        occurrenceContext = context;
    }

    /// <inheritdoc />
    public Occurrence Create(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));
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

    /// <inheritdoc />
    public Occurrence Update(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));

        try
        {
            occurrenceContext.Occurrences.Update(occurrence);
            occurrenceContext.SaveChanges();
            return occurrence;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to update occurrence", ex);
        }
    }

    /// <inheritdoc />
    public Occurrence? Delete(int id)
    {
        var existing = occurrenceContext.Occurrences.Find(id);
        if (existing == null)
            return null;
        try
        {
            occurrenceContext.Occurrences.Remove(existing);
            occurrenceContext.SaveChanges();
            return existing;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to delete occurrence", ex);
        }
    }

    /// <inheritdoc />
    public Occurrence? GetOccurrenceById(int id)
    {
        if (id <= 0)
            return null;
        return occurrenceContext.Occurrences.AsNoTracking().FirstOrDefault(o => o.Id == id);
    }

    /// <inheritdoc />
    public List<Occurrence> GetOccurrenceByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new List<Occurrence>();

        var pattern = $"%{title.Trim()}%";
        return occurrenceContext
            .Occurrences.AsNoTracking()
            .Where(o => EF.Functions.ILike(o.Title, pattern))
            .ToList();
    }

    /// <inheritdoc />
    public List<Occurrence> GetAllOccurrences(
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortOrder,
        string filter
    )
    {
        if (pageNumber <= 0)
            pageNumber = 1;
        if (pageSize <= 0)
            pageSize = 10;
        if (pageSize > 1000)
            pageSize = 1000;

        var query = occurrenceContext.Occurrences.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var trimmed = filter.Trim();
            var pattern = $"%{trimmed}%";
            query = query.Where(o =>
                EF.Functions.ILike(o.Title ?? string.Empty, pattern)
                || EF.Functions.ILike(o.Description ?? string.Empty, pattern)
            );
        }

        var asc = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sortBy?.ToLowerInvariant())
        {
            case "title":
                query = asc ? query.OrderBy(o => o.Title) : query.OrderByDescending(o => o.Title);
                break;
            case "priority":
                query = asc
                    ? query.OrderBy(o => o.Priority)
                    : query.OrderByDescending(o => o.Priority);
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

    /// <inheritdoc />
    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return occurrenceContext.Occurrences.AsNoTracking().Where(o => o.Type == type).ToList();
    }

    /// <inheritdoc />
    public List<Occurrence> GetOccurrencesByStatus(OccurrenceStatus status)
    {
        return occurrenceContext.Occurrences.AsNoTracking().Where(o => o.Status == status).ToList();
    }

    /// <inheritdoc />
    public List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority)
    {
        return occurrenceContext
            .Occurrences.AsNoTracking()
            .Where(o => o.Priority == priority)
            .ToList();
    }

    /// <inheritdoc />
    public List<Occurrence> GetAllActiveOccurrences(
        int pageNumber,
        int pageSize,
        OccurrenceType? type,
        PriorityLevel? priority,
        int? responsibleEntityId
    )
    {
        if (pageNumber <= 0)
            pageNumber = 1;
        if (pageSize <= 0)
            pageSize = 10;
        if (pageSize > 1000)
            pageSize = 1000;

        var query = occurrenceContext
            .Occurrences.AsNoTracking()
            .Where(o =>
                o.Status == OccurrenceStatus.ACTIVE || o.Status == OccurrenceStatus.IN_PROGRESS
            );

        if (type.HasValue)
            query = query.Where(o => o.Type == type.Value);

        if (priority.HasValue)
            query = query.Where(o => o.Priority == priority.Value);

        if (responsibleEntityId.HasValue)
            query = query.Where(o => o.ResponsibleEntityId == responsibleEntityId.Value);

        query = query.OrderByDescending(o => o.Priority).ThenByDescending(o => o.CreationDateTime);

        var skip = (pageNumber - 1) * pageSize;
        return query.Skip(skip).Take(pageSize).ToList();
    }

    /// <inheritdoc />
    public Occurrence? GetByReportId(int reportId)
    {
        if (reportId <= 0)
            return null;
        return occurrenceContext
            .Occurrences.AsNoTracking()
            .FirstOrDefault(o => o.ReportId == reportId);
    }
}

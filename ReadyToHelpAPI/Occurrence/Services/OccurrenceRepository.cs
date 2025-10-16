namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Data;
using readytohelpapi.Occurrence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class OccurrenceRepository : IOccurrenceRepository
{
    private readonly OccurrenceContext occurrenceContext;

    public OccurrenceRepository(OccurrenceContext context)
    {
        occurrenceContext = context;
    }

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

    public Occurrence? GetOccurrenceById(int id)
    {
        if (id <= 0) return null;
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .FirstOrDefault(o => o.Id == id);
    }

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

    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Type == type)
            .ToList();
    }

    public List<Occurrence> GetOccurrencesByStatus(OccurrenceStatus status)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Status == status)
            .ToList();
    }

    public List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority)
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Priority == priority)
            .ToList();
    }

    public List<Occurrence> GetAllActiveOccurrences()
    {
        return occurrenceContext.Occurrences
            .AsNoTracking()
            .Where(o => o.Status == OccurrenceStatus.ACTIVE)
            .ToList();
    }
}
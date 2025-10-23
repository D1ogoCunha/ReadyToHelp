namespace readytohelpapi.ResponsibleEntity.Services;

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;

/// <summary>
/// Implements the responsible entity service.
/// </summary>
public class ResponsibleEntityService : IResponsibleEntityService
{
    private readonly AppDbContext _context;
    private static readonly GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponsibleEntityService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ResponsibleEntityService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Finds the responsible entity for a given occurrence type and location.
    /// </summary>
    /// <param name="occurrenceType">The type of occurrence.</param>
    /// <param name="latitude">The latitude of the occurrence.</param>
    /// <param name="longitude">The longitude of the occurrence.</param>
    /// <returns>The responsible entity or null if none found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when latitude or longitude is out of range.</exception>
    public ResponsibleEntity? FindResponsibleEntity(OccurrenceType occurrenceType, double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        var entityType = occurrenceType.GetResponsibleEntityType();

        var point = gf.CreatePoint(new Coordinate(longitude, latitude));

        var entity = _context.ResponsibleEntities
            .AsNoTracking()
            .Where(re => re.Type == entityType)
            .Where(re => re.GeoArea != null && re.GeoArea.Contains(point))
            .FirstOrDefault();

        return entity;
    }
}

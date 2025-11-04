namespace readytohelpapi.ResponsibleEntity.Services;

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;

/// <summary>
/// Implements the responsible entity service.
/// </summary>
public class ResponsibleEntityService : IResponsibleEntityService
{
    private readonly AppDbContext _context;
    private static readonly GeometryFactory geoFactory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponsibleEntityService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ResponsibleEntityService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ResponsibleEntity? FindResponsibleEntity(
        OccurrenceType occurrenceType,
        double latitude,
        double longitude
    )
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "Latitude must be between -90 and 90."
            );

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                "Longitude must be between -180 and 180."
            );

        var entityType = occurrenceType.GetResponsibleEntityType();

        var point = geoFactory.CreatePoint(new Coordinate(longitude, latitude));

        var entity = _context
            .ResponsibleEntities.AsNoTracking()
            .Where(re => re.Type == entityType)
            .Where(re => re.GeoArea != null && re.GeoArea.Contains(point))
            .FirstOrDefault();

        return entity;
    }
}

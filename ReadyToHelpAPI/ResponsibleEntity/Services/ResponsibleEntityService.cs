namespace readytohelpapi.ResponsibleEntity.Services;

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;

public class ResponsibleEntityService : IResponsibleEntityService
{
    private readonly AppDbContext _context;

    public ResponsibleEntityService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ResponsibleEntity? FindResponsibleEntity(OccurrenceType occurrenceType, double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        // 1. Mapeia o tipo de ocorrência para o tipo de entidade responsável
        var entityType = occurrenceType.GetResponsibleEntityType();

        // 2. Cria o ponto geográfico
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        // 3. Busca entidade do tipo correto cuja área contém o ponto
        var entity = _context.ResponsibleEntities
            .AsNoTracking()
            .Where(re => re.Type == entityType)
            .Where(re => re.GeoArea != null && re.GeoArea.Contains(point))
            .FirstOrDefault();

        return entity;
    }
}

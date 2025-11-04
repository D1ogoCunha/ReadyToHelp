namespace readytohelpapi.ResponsibleEntity.Tests;

using NetTopologySuite;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.ResponsibleEntity.Tests.Fixtures;
using Xunit;

/// <summary>
/// This class contains all integration tests related to the ResponsibleEntityService.
/// </summary>
[Trait("Category", "Integration")]
public class TestResponsibleEntityService : IClassFixture<DbFixture>
{
    private static readonly GeometryFactory Gf4326 =
        NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private readonly AppDbContext ctx;
    private readonly IResponsibleEntityService svc;

    private static Polygon MakeSquare(double lon, double lat, double halfSizeDeg = 0.01)
    {
        var coords = new[]
        {
            new Coordinate(lon - halfSizeDeg, lat - halfSizeDeg),
            new Coordinate(lon - halfSizeDeg, lat + halfSizeDeg),
            new Coordinate(lon + halfSizeDeg, lat + halfSizeDeg),
            new Coordinate(lon + halfSizeDeg, lat - halfSizeDeg),
            new Coordinate(lon - halfSizeDeg, lat - halfSizeDeg),
        };
        return Gf4326.CreatePolygon(coords);
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestResponsibleEntityService"/> class.
    /// </summary>
    /// <param name="fixture">the database fixture</param>
    public TestResponsibleEntityService(DbFixture fixture)
    {
        fixture.ResetDatabase();
        ctx = fixture.Context;
        svc = new ResponsibleEntityService(ctx);
    }

    /// <summary>
    ///  Tests the FindResponsibleEntity method when no entities exist in the database.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_NoEntities_ReturnsNull()
    {
        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    /// <summary>
    ///  Tests the FindResponsibleEntity method when an entity of a different type exists.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_TypeMismatch_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var otherType =
            ResponsibleEntityType.POLICIA == expectedType
                ? ResponsibleEntityType.BOMBEIROS
                : ResponsibleEntityType.POLICIA;

        var poly = MakeSquare(-8.6, 41.15);
        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "Wrong Type",
                Email = "x@x",
                Address = "addr",
                ContactPhone = 111,
                Type = otherType,
                GeoArea = Gf4326.CreateMultiPolygon(new[] { poly }),
            }
        );
        ctx.SaveChanges();
        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    /// <summary>
    ///  Tests the FindResponsibleEntity method when an entity has a null GeoArea.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_GeoAreaNull_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();

        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "No Area",
                Email = "x@x",
                Address = "addr",
                ContactPhone = 111,
                Type = expectedType,
                GeoArea = null,
            }
        );
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    /// <summary>
    ///  Tests the FindResponsibleEntity method when a point is inside the ResponsibleEntity's GeoArea.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_PointInsidePolygon_ReturnsEntity()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var centerLat = 41.1500;
        var centerLon = -8.6000;
        var poly = MakeSquare(centerLon, centerLat);

        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "Ent A",
                Email = "a@x",
                Address = "addr",
                ContactPhone = 111,
                Type = expectedType,
                GeoArea = Gf4326.CreateMultiPolygon(new[] { poly }),
            }
        );
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, centerLat, centerLon);

        Assert.NotNull(res);
        Assert.Equal("Ent A", res!.Name);
    }

    /// <summary>
    ///  Tests the FindResponsibleEntity method when a point is outside the ResponsibleEntity's GeoArea.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_PointOutsidePolygon_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var poly = MakeSquare(-8.6, 41.15, 0.005);

        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "Ent A",
                Email = "a@x",
                Address = "addr",
                ContactPhone = 111,
                Type = expectedType,
                GeoArea = Gf4326.CreateMultiPolygon(new[] { poly }),
            }
        );
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.30, -8.80);
        Assert.Null(res);
    }

    /// <summary>
    /// Tests if the FindResponsibleEntity method returns the responsible entity when the searched point
    /// is inside any part of the entity's geo area.
    /// </summary>
    [Fact]
    public void FindResponsibleEntity_MultiPolygon_Matches_ReturnsEntity()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var poly1 = MakeSquare(-8.60, 41.15, 0.005);
        var poly2 = MakeSquare(-8.40, 41.20, 0.01);
        var multi = Gf4326.CreateMultiPolygon(new[] { poly1, poly2 });

        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "Ent MP",
                Email = "mp@x",
                Address = "addr",
                ContactPhone = 111,
                Type = expectedType,
                GeoArea = multi,
            }
        );
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.205, -8.395);

        Assert.NotNull(res);
        Assert.Equal("Ent MP", res!.Name);
    }
}

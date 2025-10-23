namespace readytohelpapi.ResponsibleEntity.Tests;

using System;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using Xunit;

public class TestResponsibleEntityService
{
    private static readonly GeometryFactory Gf4326 =
        NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private readonly AppDbContext ctx;
    private readonly IResponsibleEntityService svc;

    public TestResponsibleEntityService()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ResponsibleEntityServiceTests_" + Guid.NewGuid())
            .Options;
        ctx = new AppDbContext(opts);
        svc = new ResponsibleEntityService(ctx);
    }

    private static Polygon MakeSquare(double lon, double lat, double halfSizeDeg = 0.01)
    {
        var coords = new[]
        {
            new Coordinate(lon - halfSizeDeg, lat - halfSizeDeg),
            new Coordinate(lon - halfSizeDeg, lat + halfSizeDeg),
            new Coordinate(lon + halfSizeDeg, lat + halfSizeDeg),
            new Coordinate(lon + halfSizeDeg, lat - halfSizeDeg),
            new Coordinate(lon - halfSizeDeg, lat - halfSizeDeg)
        };
        return Gf4326.CreatePolygon(coords);
    }

    [Fact]
    public void FindResponsibleEntity_InvalidLatitude_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 123.0, -8.2));
    }

    [Fact]
    public void FindResponsibleEntity_InvalidLongitude_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.2, -999.0));
    }

    [Fact]
    public void FindResponsibleEntity_NoEntities_ReturnsNull()
    {
        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_TypeMismatch_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var otherType = ResponsibleEntityType.POLICIA == expectedType
            ? ResponsibleEntityType.BOMBEIROS
            : ResponsibleEntityType.POLICIA;

        var poly = MakeSquare(-8.6, 41.15);
        ctx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Name = "Wrong Type",
            Email = "x@x",
            Address = "addr",
            ContactPhone = 111,
            Type = otherType,
            GeoArea = Gf4326.CreateMultiPolygon(new[] { poly })
        });
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_GeoAreaNull_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();

        ctx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Name = "No Area",
            Email = "x@x",
            Address = "addr",
            ContactPhone = 111,
            Type = expectedType,
            GeoArea = null
        });
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.6);
        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_PointInsidePolygon_ReturnsEntity()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var centerLat = 41.1500;
        var centerLon = -8.6000;
        var poly = MakeSquare(centerLon, centerLat);

        ctx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Name = "Ent A",
            Email = "a@x",
            Address = "addr",
            ContactPhone = 111,
            Type = expectedType,
            GeoArea = Gf4326.CreateMultiPolygon(new[] { poly })
        });
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, centerLat, centerLon);

        Assert.NotNull(res);
        Assert.Equal("Ent A", res!.Name);
    }

    [Fact]
    public void FindResponsibleEntity_PointOutsidePolygon_ReturnsNull()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var poly = MakeSquare(-8.6, 41.15, 0.005);

        ctx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Name = "Ent A",
            Email = "a@x",
            Address = "addr",
            ContactPhone = 111,
            Type = expectedType,
            GeoArea = Gf4326.CreateMultiPolygon(new[] { poly })
        });
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.30, -8.80);
        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_MultiPolygon_Matches_ReturnsEntity()
    {
        var expectedType = OccurrenceType.ROAD_DAMAGE.GetResponsibleEntityType();
        var poly1 = MakeSquare(-8.60, 41.15, 0.005);
        var poly2 = MakeSquare(-8.40, 41.20, 0.01);
        var multi = Gf4326.CreateMultiPolygon(new[] { poly1, poly2 });

        ctx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Name = "Ent MP",
            Email = "mp@x",
            Address = "addr",
            ContactPhone = 111,
            Type = expectedType,
            GeoArea = multi
        });
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.205, -8.395);

        Assert.NotNull(res);
        Assert.Equal("Ent MP", res!.Name);
    }
}
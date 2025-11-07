namespace readytohelpapi.ResponsibleEntity.Tests;

using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using Xunit;
using static NetTopologySuite.NtsGeometryServices;

/// <summary>
/// This class contains all unit test related to the ResponsibleEntityService.
/// </summary>
[Trait("Category", "Unit")]
public class TestResponsibleEntityService_Unit
{
    private static GeometryFactory Gf4326 => Instance.CreateGeometryFactory(4326);

    private static Polygon MakeSquare(double lonCenter, double latCenter, double halfSizeDeg)
    {
        var minX = lonCenter - halfSizeDeg;
        var maxX = lonCenter + halfSizeDeg;
        var minY = latCenter - halfSizeDeg;
        var maxY = latCenter + halfSizeDeg;

        var coords = new[]
        {
            new Coordinate(minX, minY),
            new Coordinate(minX, maxY),
            new Coordinate(maxX, maxY),
            new Coordinate(maxX, minY),
            new Coordinate(minX, minY),
        };
        return Gf4326.CreatePolygon(Gf4326.CreateLinearRing(coords));
    }

    [Fact]
    public void FindResponsibleEntity_EmptyDatabase_ReturnsNull()
    {
        using var ctx = NewInMemoryContext();
        var svc = new ResponsibleEntityService(ctx);

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.15, -8.61);

        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_AllEntitiesOutside_ReturnsNull()
    {
        using var ctx = NewInMemoryContext();
        var svc = new ResponsibleEntityService(ctx);

        var lat = 41.15;
        var lon = -8.61;

        foreach (ResponsibleEntityType t in Enum.GetValues(typeof(ResponsibleEntityType)))
        {
            ctx.ResponsibleEntities.Add(
                new ResponsibleEntity
                {
                    Name = $"E_{t}",
                    Type = t,
                    GeoArea = Gf4326.CreateMultiPolygon(
                        new[] { MakeSquare(lon + 10, lat + 10, 0.02) }
                    ),
                }
            );
        }
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, lat, lon);

        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_PointInsideOneOfTheAreas_ReturnsEntity()
    {
        using var ctx = NewInMemoryContext();
        var svc = new ResponsibleEntityService(ctx);

        var lat = 41.15;
        var lon = -8.61;

        foreach (ResponsibleEntityType t in Enum.GetValues(typeof(ResponsibleEntityType)))
        {
            ctx.ResponsibleEntities.Add(
                new ResponsibleEntity
                {
                    Name = $"Hit_{t}",
                    Type = t,
                    GeoArea = Gf4326.CreateMultiPolygon(new[] { MakeSquare(lon, lat, 0.05) }),
                }
            );
        }
        ctx.SaveChanges();

        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, lat, lon);

        Assert.NotNull(res);
        Assert.NotNull(res!.GeoArea);
        Assert.True(res.GeoArea!.Contains(Gf4326.CreatePoint(new Coordinate(lon, lat))));
    }

    [Fact]
    public void FindResponsibleEntity_PointOnBoundary_ReturnsNull()
    {
        using var ctx = NewInMemoryContext();
        var svc = new ResponsibleEntityService(ctx);

        var lat = 41.15;
        var lon = -8.61;
        var half = 0.02;

        foreach (ResponsibleEntityType t in Enum.GetValues(typeof(ResponsibleEntityType)))
        {
            ctx.ResponsibleEntities.Add(
                new ResponsibleEntity
                {
                    Name = $"B_{t}",
                    Type = t,
                    GeoArea = Gf4326.CreateMultiPolygon(new[] { MakeSquare(lon, lat, half) }),
                }
            );
        }
        ctx.SaveChanges();

        var boundaryLon = lon + half;
        var res = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, lat, boundaryLon);

        Assert.Null(res);
    }

    [Fact]
    public void FindResponsibleEntity_LatLonBounds_DoNotThrow_AndReturnNull()
    {
        using var ctx = NewInMemoryContext();
        var svc = new ResponsibleEntityService(ctx);

        var r1 = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, -90.0, -180.0);
        var r2 = svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 90.0, 180.0);

        Assert.Null(r1);
        Assert.Null(r2);
    }

    private static AppDbContext NewInMemoryContext()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"resp-ent-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public void FindResponsibleEntity_InvalidLatitude_Throws()
    {
        var options = new DbContextOptions<AppDbContext>();
        var mockCtx = new Mock<AppDbContext>(options);
        var svc = new ResponsibleEntityService(mockCtx.Object);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 123.0, -8.2)
        );
    }

    [Fact]
    public void FindResponsibleEntity_InvalidLongitude_Throws()
    {
        var options = new DbContextOptions<AppDbContext>();
        var mockCtx = new Mock<AppDbContext>(options);
        var svc = new ResponsibleEntityService(mockCtx.Object);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.FindResponsibleEntity(OccurrenceType.ROAD_DAMAGE, 41.2, -999.0)
        );
    }
}

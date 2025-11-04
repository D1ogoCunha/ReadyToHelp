namespace readytohelpapi.ResponsibleEntity.Tests;

using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Services;
using Xunit;

/// <summary>
/// This class contains all unit test related to the ResponsibleEntityService.
/// </summary>
[Trait("Category", "Unit")]
public class TestResponsibleEntityService_Unit
{
    /// <summary>
    /// Tests the FindResponsibleEntity method with invalid latitude.
    /// </summary>
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

    /// <summary>
    /// Tests the FindResponsibleEntity method with invalid longitude.
    /// </summary>
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

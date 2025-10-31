namespace readytohelpapi.Occurrence.Tests;

using Xunit;
using Moq;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.Occurrence.Tests.Fixtures;
using readytohelpapi.ResponsibleEntity.Models;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Integration")]
public class TestOccurrenceApiController_Integration : IClassFixture<DbFixture>
{
    private readonly AppDbContext _context;
    private readonly IOccurrenceService _service;
    private readonly IOccurrenceRepository _repo;
    private readonly OccurrenceApiController _controller;

    public TestOccurrenceApiController_Integration(DbFixture fixture)
    {
        _context = fixture.Context;
        fixture.ResetDatabase();

        _repo = new OccurrenceRepository(_context);
        
        var fakeResponsibleService = new Mock<IResponsibleEntityService>();
        fakeResponsibleService
            .Setup(s => s.FindResponsibleEntity(It.IsAny<OccurrenceType>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns((ResponsibleEntity)null);

        _service = new OccurrenceServiceImpl(_repo, fakeResponsibleService.Object);

        _controller = new OccurrenceApiController(_service);
    }

    [Fact]
    public void CreateOccurrence_StoresInDatabase()
    {
        var input = new Occurrence
        {
            Title = "Flood A1",
            Description = "Water rising",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.HIGH,
            ProximityRadius = 10,
            Location = new GeoPoint.Models.GeoPoint { Latitude = 40.123, Longitude = -8.321 }
        };

        var result = _controller.Create(input);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);

        var created = Assert.IsType<Occurrence>(createdAt.Value);
        Assert.True(created.Id > 0);

        var dbItem = _context.Occurrences.FirstOrDefault(o => o.Id == created.Id);
        Assert.NotNull(dbItem);
        Assert.Equal("Flood A1", dbItem.Title);
    }

    [Fact]
    public void GetById_ReturnsStoredOccurrence()
    {
        var occ = new Occurrence
        {
            Title = "Fire A2",
            Description = "Forest fire",
            Type = OccurrenceType.FOREST_FIRE,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 20,
            Location = new GeoPoint.Models.GeoPoint { Latitude = 41, Longitude = -8 }
        };

        var stored = _repo.Create(occ);

        var result = _controller.GetById(stored.Id);
        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var dto = Assert.IsType<OccurrenceDetailsDto>(ok.Value);
        Assert.Equal(stored.Id, dto.Id);
        Assert.Equal("Fire A2", dto.Title);
    }
}

namespace readytohelpapi.Occurrence.Tests;

using Xunit;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using System.Linq;
using System;
using Moq;

[Trait("Category", "Integration")]
public class TestOccurrenceService_Integration : IClassFixture<DbFixture>
{
    private readonly AppDbContext _ctx;
    private readonly IOccurrenceRepository _repo;
    private readonly IOccurrenceService _service;
    private readonly ResponsibleEntity _seededRe;

    public TestOccurrenceService_Integration(DbFixture fixture)
    {
        _ctx = fixture.Context;
        fixture.ResetDatabase();

        _repo = new OccurrenceRepository(_ctx);

        _seededRe = new ResponsibleEntity
        {
            Name = "INEM Test",
            Email = "inem@test.com",
            Address = "Av. Central",
            ContactPhone = 900000000,
            Type = ResponsibleEntityType.INEM
        };

        _ctx.ResponsibleEntities.Add(_seededRe);
        _ctx.SaveChanges();

        var responsibleMock = new Mock<IResponsibleEntityService>();
        responsibleMock.Setup(r => r.FindResponsibleEntity(
            It.IsAny<OccurrenceType>(),
            It.IsAny<double>(),
            It.IsAny<double>()
        )).Returns(_seededRe);

        _service = new OccurrenceServiceImpl(_repo, responsibleMock.Object);
    }

    private Occurrence BuildOccurrence()
    {
        return new Occurrence
        {
            Title = "Flood Test",
            Description = "River flooding area",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.HIGH,
            Location = new GeoPoint.Models.GeoPoint { Latitude = 40.0, Longitude = -8.0 },
            ReportCount = 1,
            CreationDateTime = DateTime.UtcNow,
            Status = OccurrenceStatus.WAITING,
            ProximityRadius = 100
        };
    }

    [Fact]
    public void Create_AssignsResponsibleEntity()
    {
        var occ = BuildOccurrence();

        var saved = _service.CreateAdminOccurrence(occ);

        Assert.NotNull(saved);
        Assert.True(saved.Id > 0);
        Assert.Equal(_seededRe.Id, saved.ResponsibleEntityId);
        Assert.True(_ctx.Occurrences.Any(o => o.Id == saved.Id));
    }

    [Fact]
    public void Create_Throws_WhenNull()
    {
        Assert.ThrowsAny<ArgumentException>(() => _service.CreateAdminOccurrence(null!));
    }

    [Fact]
    public void GetById_ReturnsSameRecord()
    {
        var occ = _service.CreateAdminOccurrence(BuildOccurrence());

        var retrieved = _service.GetOccurrenceById(occ.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(occ.Id, retrieved.Id);
        Assert.Equal(occ.Title, retrieved.Title);
    }

    [Fact]
    public void Update_PersistsChanges()
    {
        var occ = _service.CreateAdminOccurrence(BuildOccurrence());
        occ.Title = "Updated Title";

        var updated = _service.Update(occ);

        var db = _ctx.Occurrences.First(o => o.Id == occ.Id);

        Assert.Equal("Updated Title", db.Title);
        Assert.Equal(updated.Title, db.Title);
    }

    [Fact]
    public void Delete_RemovesRecord()
    {
        var occ = _service.CreateAdminOccurrence(BuildOccurrence());

        _service.Delete(occ.Id);

        Assert.False(_ctx.Occurrences.Any(o => o.Id == occ.Id));
    }
}


using Microsoft.EntityFrameworkCore;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

public class TestOccurrenceRepository : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly IOccurrenceRepository repo;

    public TestOccurrenceRepository(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        repo = new OccurrenceRepository(this.fixture.Context);
    }

    [Fact]
    public void Create_Valid_ReturnsCreated()
    {
        var o = new Models.Occurrence { Title = "A", Description = "B", Type = OccurrenceType.FOREST_FIRE, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        var created = repo.Create(o);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public void Create_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => repo.Create(null!));
    }

    [Fact]
    public void GetById_WhenExists_ReturnsEntity()
    {
        var o = new Models.Occurrence { Title = "X", Description = "Y", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.MEDIUM, ProximityRadius = 50 };
        var created = repo.Create(o);

        var got = repo.GetOccurrenceById(created.Id);
        Assert.NotNull(got);
        Assert.Equal(created.Id, got!.Id);
    }

    [Fact]
    public void GetById_WhenNotExists_ReturnsNull()
    {
        var got = repo.GetOccurrenceById(999999);
        Assert.Null(got);
    }

    [Fact]
    public void GetByTitle_PartialMatch_ReturnsList()
    {
        repo.Create(new Models.Occurrence { Title = "Road obstruction", Description = "desc", Type = OccurrenceType.ROAD_OBSTRUCTION, Priority = PriorityLevel.LOW, ProximityRadius = 5 });
        var list = repo.GetOccurrenceByTitle("road");
        Assert.NotEmpty(list);
    }

    [Fact]
    public void GetAllOccurrences_FilterAndSort_Works()
    {
        repo.Create(new Models.Occurrence { Title = "Alpha", Description = "a", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 });
        repo.Create(new Models.Occurrence { Title = "Zulu", Description = "z", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.HIGH, ProximityRadius = 10 });

        var page = repo.GetAllOccurrences(1, 10, "title", "desc", "l");
        Assert.True(page.Count >= 1);
        Assert.Equal("Zulu", page.First().Title);
    }

    [Fact]
    public void Update_Existing_UpdatesFields()
    {
        var o = repo.Create(new Models.Occurrence { Title = "Old", Description = "OldD", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 });
        o.Title = "New";
        o.Priority = PriorityLevel.HIGH;

        var updated = repo.Update(o);
        Assert.Equal("New", updated.Title);
        Assert.Equal(PriorityLevel.HIGH, updated.Priority);
    }

    [Fact]
    public void Update_NotExisting_ThrowsKeyNotFound()
    {
        var ghost = new Models.Occurrence { Id = 999999, Title = "X", Description = "Y", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        Assert.Throws<KeyNotFoundException>(() => repo.Update(ghost));
    }

    [Fact]
    public void Delete_Existing_RemovesAndReturns()
    {
        var o = repo.Create(new Models.Occurrence { Title = "Del", Description = "D", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 });

        var deleted = repo.Delete(o.Id);
        Assert.NotNull(deleted);
        Assert.Null(repo.GetOccurrenceById(o.Id));
    }

    [Fact]
    public void Delete_NotExisting_ThrowsKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => repo.Delete(987654));
    }
}
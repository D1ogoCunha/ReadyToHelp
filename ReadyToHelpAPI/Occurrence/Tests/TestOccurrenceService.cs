using Moq;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

public class TestOccurrenceService
{
    private readonly Mock<IOccurrenceRepository> mockRepo = new();
    private readonly IOccurrenceService service;

    public TestOccurrenceService()
    {
        service = new OccurrenceServiceImpl(mockRepo.Object);
    }

    [Fact]
    public void Create_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => service.Create(null!));
    }

    [Fact]
    public void Create_InvalidTitle_Throws()
    {
        var o = new Models.Occurrence { Title = "", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        Assert.Throws<ArgumentException>(() => service.Create(o));
    }

    [Fact]
    public void Create_Valid_ReturnsCreated()
    {
        var input = new Models.Occurrence { Title = "Ok", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        var created = new Models.Occurrence { Id = 10, Title = "Ok", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };

        mockRepo.Setup(r => r.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var res = service.Create(input);
        Assert.Equal(10, res.Id);
    }

    [Fact]
    public void Create_RepoThrows_WrapsIntoInvalidOperation()
    {
        var input = new Models.Occurrence { Title = "Ok", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        mockRepo.Setup(r => r.Create(It.IsAny<Models.Occurrence>())).Throws(new Exception("DB"));

        Assert.Throws<InvalidOperationException>(() => service.Create(input));
    }

    [Fact]
    public void GetById_Invalid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => service.GetOccurrenceById(0));
    }

    [Fact]
    public void GetById_NotFound_ThrowsKeyNotFound()
    {
        mockRepo.Setup(r => r.GetOccurrenceById(123)).Returns((Models.Occurrence?)null);
        Assert.Throws<KeyNotFoundException>(() => service.GetOccurrenceById(123));
    }

    [Fact]
    public void Update_InvalidId_Throws()
    {
        var o = new Models.Occurrence { Id = 0, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        Assert.Throws<ArgumentException>(() => service.Update(o));
    }

    [Fact]
    public void Update_NotFound_PropagatesKeyNotFound()
    {
        var o = new Models.Occurrence { Id = 999, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>())).Throws(new KeyNotFoundException("not found"));

        Assert.Throws<KeyNotFoundException>(() => service.Update(o));
    }

    [Fact]
    public void Update_RepoThrows_WrapsInvalidOperation()
    {
        var o = new Models.Occurrence { Id = 5, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>())).Throws(new Exception("DB"));

        Assert.Throws<InvalidOperationException>(() => service.Update(o));
    }

    [Fact]
    public void Delete_InvalidId_Throws()
    {
        Assert.Throws<ArgumentException>(() => service.Delete(0));
    }

    [Fact]
    public void Delete_RepoReturnsNull_ThrowsKeyNotFound()
    {
        mockRepo.Setup(r => r.Delete(123)).Returns((Models.Occurrence?)null);
        Assert.Throws<KeyNotFoundException>(() => service.Delete(123));
    }

    [Fact]
    public void Delete_RepoThrows_WrapsInvalidOperation()
    {
        mockRepo.Setup(r => r.Delete(5)).Throws(new Exception("DB"));
        Assert.Throws<InvalidOperationException>(() => service.Delete(5));
    }

    [Fact]
    public void Delete_Valid_ReturnsEntity()
    {
        var o = new Models.Occurrence { Id = 7, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10 };
        mockRepo.Setup(r => r.Delete(7)).Returns(o);

        var res = service.Delete(7);
        Assert.Equal(7, res.Id);
    }

    [Fact]
    public void GetAllOccurrences_RepoReturnsNull_ReturnsEmpty()
    {
        mockRepo.Setup(r => r.GetAllOccurrences(1, 10, "Title", "asc", ""))
                .Returns((List<Models.Occurrence>?)null);

        var list = service.GetAllOccurrences(1, 10, "Title", "asc", "");
        Assert.Empty(list);
    }
}
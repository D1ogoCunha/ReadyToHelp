using Moq;
using GeoPointModel = readytohelpapi.GeoPoint.Models.GeoPoint;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

/// <summary>
///  This class contains unit tests related to the occurrence service.
/// </summary>
public class TestOccurrenceServiceTest
{
    private readonly Mock<IOccurrenceRepository> mockRepo;
    private readonly IOccurrenceService service;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestOccurrenceServiceTest"/> class.
    /// </summary>
    public TestOccurrenceServiceTest()
    {
        mockRepo = new Mock<IOccurrenceRepository>();
        service = new OccurrenceServiceImpl(mockRepo.Object);
    }

    /// <summary>
    ///  Tests Create with a null occurrence.
    /// </summary>
    [Fact]
    public void Create_NullOccurrence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => service.Create(null!));
    }

    /// <summary>
    ///  Tests Create with an empty title.
    /// </summary>
    [Fact]
    public void Create_EmptyTitle_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Title = "",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create with an empty description.
    /// </summary>
    [Fact]
    public void Create_EmptyDescription_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create with an invalid occurrence type.
    /// </summary>
    [Fact]
    public void Create_InvalidType_ThrowsArgumentOutOfRangeException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = (OccurrenceType)999,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create with an invalid priority level.
    /// </summary>
    [Fact]
    public void Create_InvalidPriority_ThrowsArgumentOutOfRangeException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = (PriorityLevel)999,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create with a non-positive proximity radius.
    /// </summary>
    [Fact]
    public void Create_NonPositiveRadius_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 0,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("Proximity radius", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create with negative counters and IDs.
    /// </summary>
    [Fact]
    public void Create_NegativeCountersAndIds_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            ReportCount = -1,
            ReportId = -5,
            ResponsibleEntityId = -3,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create when EndDateTime is earlier than CreationDateTime.
    /// </summary>
    [Fact]
    public void Create_EndDateBeforeCreation_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            EndDateTime = DateTime.UtcNow.AddMinutes(-5),
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("EndDateTime", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Create_RepositoryThrows_WrapsInvalidOperationException()
    {
        var o = new Models.Occurrence
        {
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        mockRepo.Setup(r => r.Create(It.IsAny<Models.Occurrence>())).Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create with valid input.
    /// </summary>
    [Fact]
    public void Create_Valid_ReturnsCreatedOccurrence()
    {
        var input = new Models.Occurrence
        {
            Title = "Ok",
            Description = "Desc",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 25,
            Location = new GeoPointModel { Latitude = 40.1, Longitude = -8.1 }
        };
        var created = new Models.Occurrence
        {
            Id = 10,
            Title = "Ok",
            Description = "Desc",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 25,
            Location = new GeoPointModel { Latitude = 40.1, Longitude = -8.1 }
        };
        mockRepo.Setup(r => r.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var result = service.Create(input);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
    }

    /// <summary>
    ///  Tests Create allows null ReportId.
    /// </summary>
    [Fact]
    public void Create_AllowsNullReportId()
    {
        var input = new Models.Occurrence
        {
            Title = "Ok",
            Description = "Desc",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.WAITING,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 25,
            ReportCount = 0,
            ReportId = null,
            ResponsibleEntityId = 0,
            Location = new GeoPointModel { Latitude = 40.1, Longitude = -8.1 }
        };

        var created = new Models.Occurrence { Id = 11, Title = "Ok", ReportId = null };
        mockRepo.Setup(r => r.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var result = service.Create(input);

        Assert.Equal(11, result.Id);
        Assert.Null(result.ReportId);
    }

    /// <summary>
    ///  Tests Update allows null ReportId.
    /// </summary>
    [Fact]
    public void Update_AllowsNullReportId()
    {
        var input = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            ReportCount = 0,
            ReportId = null,
            ResponsibleEntityId = 0,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };

        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>())).Returns(input);

        var result = service.Update(input);

        Assert.Equal(5, result.Id);
        Assert.Null(result.ReportId);
    }

    /// <summary>
    ///  Tests Update with a null occurrence.
    /// </summary>
    [Fact]
    public void Update_NullOccurrence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => service.Update(null!));
    }

    /// <summary>
    ///  Tests Update with an invalid occurrence ID.
    /// </summary>
    [Fact]
    public void Update_InvalidId_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Id = 0,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("Invalid occurrence ID", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an empty title.
    /// </summary>
    [Fact]
    public void Update_EmptyTitle_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an empty description.
    /// </summary>
    [Fact]
    public void Update_EmptyDescription_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with invalid enums and values.
    /// </summary>
    [Fact]
    public void Update_InvalidEnumsAndValues_Throws()
    {
        var invalidType = new Models.Occurrence
        {
            Id = 1,
            Title = "t",
            Description = "d",
            Type = (OccurrenceType)999,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(invalidType));

        var invalidPriority = new Models.Occurrence
        {
            Id = 1,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = (PriorityLevel)999,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(invalidPriority));

        var nonPositiveRadius = new Models.Occurrence
        {
            Id = 1,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 0,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentException>(() => service.Update(nonPositiveRadius));

        var negativeCounts = new Models.Occurrence
        {
            Id = 1,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            ReportCount = -1,
            ReportId = -2,
            ResponsibleEntityId = -3,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        Assert.Throws<ArgumentException>(() => service.Update(negativeCounts));
    }

    /// <summary>
    ///  Tests Update when EndDateTime is earlier than CreationDateTime.
    /// </summary>
    [Fact]
    public void Update_EndDateBeforeCreation_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            CreationDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddMinutes(-1),
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("EndDateTime", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with a null location.
    /// </summary>
    [Fact]
    public void Update_NullLocation_ThrowsArgumentException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = null!
        };
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("location", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an invalid latitude.
    /// </summary>
    [Fact]
    public void Update_InvalidLatitude_ThrowsArgumentOutOfRangeException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 100, Longitude = 0 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update with an invalid longitude.
    /// </summary>
    [Fact]
    public void Update_InvalidLongitude_ThrowsArgumentOutOfRangeException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 0, Longitude = 200 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update when repository returns null (not found).
    /// </summary>
    [Fact]
    public void Update_NotFound_ThrowsKeyNotFoundException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>()))
                .Returns((Models.Occurrence)null!);

        Assert.Throws<KeyNotFoundException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update when repository throws an exception.
    /// </summary>
    [Fact]
    public void Update_RepositoryThrows_WrapsInvalidOperationException()
    {
        var o = new Models.Occurrence
        {
            Id = 5,
            Title = "t",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>()))
                .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update with valid input.
    /// </summary>
    [Fact]
    public void Update_Valid_ReturnsUpdatedOccurrence()
    {
        var updated = new Models.Occurrence
        {
            Id = 5,
            Title = "updated",
            Description = "d2",
            Type = OccurrenceType.FOREST_FIRE,
            Priority = PriorityLevel.HIGH,
            ProximityRadius = 50,
            Location = new GeoPointModel { Latitude = 41, Longitude = -8.2 }
        };
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>())).Returns(updated);

        var result = service.Update(updated);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("updated", result.Title);
    }

    // DELETE

    /// <summary>
    ///  Tests Delete with an invalid ID.
    /// </summary>
    [Fact]
    public void Delete_InvalidId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => service.Delete(0));
    }

    /// <summary>
    ///  Tests Delete when repository returns null (not found).
    /// </summary>
    [Fact]
    public void Delete_NotFound_ThrowsKeyNotFoundException()
    {
        mockRepo.Setup(r => r.Delete(123)).Returns((Models.Occurrence?)null);
        Assert.Throws<KeyNotFoundException>(() => service.Delete(123));
    }

    /// <summary>
    ///  Tests Delete when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Delete_RepositoryThrows_WrapsInvalidOperationException()
    {
        mockRepo.Setup(r => r.Delete(It.IsAny<int>())).Throws(new Exception("DB error"));
        Assert.Throws<InvalidOperationException>(() => service.Delete(5));
    }

    /// <summary>
    ///  Tests Delete with a valid ID.
    /// </summary>
    [Fact]
    public void Delete_Valid_ReturnsDeletedOccurrence()
    {
        var o = new Models.Occurrence { Id = 9, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10, Location = new GeoPointModel { Latitude = 40, Longitude = -8 } };
        mockRepo.Setup(r => r.Delete(9)).Returns(o);

        var result = service.Delete(9);

        Assert.NotNull(result);
        Assert.Equal(9, result.Id);
    }

    // GET BY ID

    /// <summary>
    ///  Tests GetOccurrenceById with an invalid ID.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_InvalidId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => service.GetOccurrenceById(0));
    }

    /// <summary>
    ///  Tests GetOccurrenceById when the occurrence is not found.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_NotFound_ThrowsKeyNotFoundException()
    {
        mockRepo.Setup(r => r.GetOccurrenceById(999)).Returns((Models.Occurrence?)null);
        Assert.Throws<KeyNotFoundException>(() => service.GetOccurrenceById(999));
    }

    /// <summary>
    ///  Tests GetOccurrenceById with a valid ID.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_Valid_ReturnsOccurrence()
    {
        var o = new Models.Occurrence { Id = 7, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10, Location = new GeoPointModel { Latitude = 40, Longitude = -8 } };
        mockRepo.Setup(r => r.GetOccurrenceById(7)).Returns(o);

        var result = service.GetOccurrenceById(7);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
    }

    // GET BY TITLE

    /// <summary>
    ///  Tests GetOccurrenceByTitle with invalid title (empty).
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_Empty_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetOccurrenceByTitle(""));
        Assert.Contains("Title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetOccurrenceByTitle with a valid title.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_Valid_ReturnsOccurrences()
    {
        var list = new List<Models.Occurrence>
            {
                new Models.Occurrence { Id = 1, Title = "Flood Downtown", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10, Location = new GeoPointModel { Latitude = 40, Longitude = -8 } }
            };
        mockRepo.Setup(r => r.GetOccurrenceByTitle("Flood")).Returns(list);

        var result = service.GetOccurrenceByTitle("Flood");

        Assert.NotNull(result);
        Assert.Single(result);
    }

    // GET ALL OCCURRENCES

    /// <summary>
    ///  Tests GetAllOccurrences with invalid page number.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidPageNumber_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetAllOccurrences(0, 10, "Title", "asc", ""));
        Assert.Contains("Page number", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with invalid page size.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidPageSize_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetAllOccurrences(1, 0, "Title", "asc", ""));
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with too large page size.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_PageSizeTooLarge_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetAllOccurrences(1, 1001, "Title", "asc", ""));
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with empty sort field.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_EmptySortBy_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetAllOccurrences(1, 10, "", "asc", ""));
        Assert.Contains("Sort field", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with invalid sort order.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidSortOrder_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => service.GetAllOccurrences(1, 10, "Title", "invalid", ""));
        Assert.Contains("Sort order", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences when repository returns null.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_RepositoryReturnsNull_ReturnsEmptyList()
    {
        mockRepo.Setup(r => r.GetAllOccurrences(1, 10, "Title", "asc", ""))
                .Returns((List<Models.Occurrence>?)null);

        var result = service.GetAllOccurrences(1, 10, "Title", "asc", "");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    ///  Tests GetAllOccurrences when repository throws an exception.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_RepositoryThrows_WrapsInvalidOperationException()
    {
        mockRepo.Setup(r => r.GetAllOccurrences(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.GetAllOccurrences(1, 10, "Title", "asc", ""));
    }

    /// <summary>
    ///  Tests GetAllOccurrences with valid parameters.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_Valid_ReturnsList()
    {
        var list = new List<Models.Occurrence>
            {
                new Models.Occurrence { Id = 2, Title = "Alpha", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10, Location = new GeoPointModel { Latitude = 40, Longitude = -8 } }
            };
        mockRepo.Setup(r => r.GetAllOccurrences(1, 10, "Title", "asc", "")).Returns(list);

        var result = service.GetAllOccurrences(1, 10, "Title", "asc", "");

        Assert.Single(result);
    }

    // SIMPLE FORWARDERS

    /// <summary>
    ///  Tests GetOccurrencesByType returns repository result.
    /// </summary>
    [Fact]
    public void GetOccurrencesByType_ReturnsRepoList()
    {
        var list = new List<Models.Occurrence>
            {
                new Models.Occurrence { Id = 1, Title = "Flood", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 10, Location = new GeoPointModel { Latitude = 40, Longitude = -8 } }
            };
        mockRepo.Setup(r => r.GetOccurrencesByType(OccurrenceType.FLOOD)).Returns(list);

        var result = service.GetOccurrencesByType(OccurrenceType.FLOOD);

        Assert.Single(result);
        Assert.Equal(OccurrenceType.FLOOD, result[0].Type);
    }

    /// <summary>
    ///  Tests GetAllActiveOccurrences delegates to repository and returns active occurrences.
    /// </summary>
    [Fact]
    public void GetAllActiveOccurrences_ReturnsActiveList()
    {
        var list = new List<Models.Occurrence>
            {
                new Models.Occurrence { Id = 3, Title = "Active", Description = "d", Type = OccurrenceType.FOREST_FIRE, Priority = PriorityLevel.HIGH, ProximityRadius = 20, Status = OccurrenceStatus.ACTIVE, Location = new GeoPointModel { Latitude = 41, Longitude = -8 } }
            };
        mockRepo.Setup(r => r.GetAllActiveOccurrences(1, 50, null, null, null)).Returns(list);

        var result = service.GetAllActiveOccurrences(1, 50, null, null, null);

        Assert.Single(result);
        Assert.All(result, o => Assert.True(
            o.Status == OccurrenceStatus.ACTIVE || o.Status == OccurrenceStatus.IN_PROGRESS
        ));
    }

    /// <summary>
    ///  Tests GetOccurrencesByPriority returns repository result.
    /// </summary>
    [Fact]
    public void GetOccurrencesByPriority_ReturnsRepoList()
    {
        var list = new List<Models.Occurrence>
            {
                new Models.Occurrence { Id = 4, Title = "High", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.HIGH, ProximityRadius = 15, Location = new GeoPointModel { Latitude = 40.5, Longitude = -8.2 } }
            };
        mockRepo.Setup(r => r.GetOccurrencesByPriority(PriorityLevel.HIGH)).Returns(list);

        var result = service.GetOccurrencesByPriority(PriorityLevel.HIGH);

        Assert.Single(result);
        Assert.Equal(PriorityLevel.HIGH, result[0].Priority);
    }
}

namespace readytohelpapi.Occurrence.Tests;

using Moq;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using Xunit;

/// <summary>
///  This class contains unit tests related to the occurrence service.
/// </summary>
[Trait("Category", "Unit")]
public class TestOccurrenceService
{
    private readonly Mock<IOccurrenceRepository> mockRepo;
    private readonly Mock<IResponsibleEntityService> mockResponsibleEntityService;
    private readonly IOccurrenceService service;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestOccurrenceService"/> class.
    /// </summary>
    public TestOccurrenceService()
    {
        mockRepo = new Mock<IOccurrenceRepository>();
        mockResponsibleEntityService = new Mock<IResponsibleEntityService>();
        service = new OccurrenceServiceImpl(mockRepo.Object, mockResponsibleEntityService.Object);
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
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "",
                Description = "desc",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create with an empty description.
    /// </summary>
    [Fact]
    public void Create_EmptyDescription_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "t",
                Description = "",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create with an invalid occurrence type.
    /// </summary>
    [Fact]
    public void Create_InvalidType_ThrowsArgumentOutOfRangeException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "xxxx",
                Description = "desc",
                Type = (OccurrenceType)999,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create with negative counters and IDs.
    /// </summary>
    [Fact]
    public void Create_NegativeCountersAndIds_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                ReportCount = -1,
                ReportId = -5,
                ResponsibleEntityId = -3,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        Assert.Throws<ArgumentException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create when EndDateTime is earlier than CreationDateTime.
    /// </summary>
    [Fact]
    public void Create_EndDateBeforeCreation_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                EndDateTime = DateTime.UtcNow.AddMinutes(-5),
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Create(o));
        Assert.Contains("EndDateTime", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Create when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Create_RepositoryThrows_WrapsInvalidOperationException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo
            .Setup(r => r.Create(It.IsAny<Models.Occurrence>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.Create(o));
    }

    /// <summary>
    ///  Tests Create with valid input.
    /// </summary>
    [Fact]
    public void Create_Valid_ReturnsCreatedOccurrence()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Ok",
                Description = "Desc",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.MEDIUM,
                ProximityRadius = 25,
                Location = new GeoPoint { Latitude = 40.1, Longitude = -8.1 },
            }
        );
        var created = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 10,
                Title = "Ok",
                Description = "Desc",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.MEDIUM,
                ProximityRadius = 25,
                Location = new GeoPoint { Latitude = 40.1, Longitude = -8.1 },
            }
        );

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
        var input = new Occurrence(
            new OccurrenceCreateDto
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
                Location = new GeoPoint { Latitude = 40.1, Longitude = -8.1 },
            }
        );

        var created = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 11,
                Title = "Ok",
                ReportId = null,
            }
        );

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
        var input = new Occurrence(
            new OccurrenceCreateDto
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
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

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
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 0,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("Invalid occurrence ID", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an empty title.
    /// </summary>
    [Fact]
    public void Update_EmptyTitle_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an empty description.
    /// </summary>
    [Fact]
    public void Update_EmptyDescription_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with invalid enums and values.
    /// </summary>
    [Fact]
    public void Update_InvalidEnumsAndValues_Throws()
    {
        var invalidType = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 1,
                Title = "t",
                Description = "d",
                Type = (OccurrenceType)999,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(invalidType));

        var nonPositiveRadius = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 1,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 0,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo
            .Setup(r => r.Update(It.IsAny<Models.Occurrence>()))
            .Returns((Models.Occurrence o) => o);

        var updated = service.Update(nonPositiveRadius);
        Assert.True(updated.ProximityRadius > 0);

        var negativeCounts = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 1,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 10,
                ReportCount = -1,
                ReportId = -2,
                ResponsibleEntityId = -3,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        Assert.Throws<ArgumentException>(() => service.Update(negativeCounts));
    }

    /// <summary>
    ///  Tests Update when EndDateTime is earlier than CreationDateTime.
    /// </summary>
    [Fact]
    public void Update_EndDateBeforeCreation_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                CreationDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddMinutes(-1),
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("EndDateTime", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with a null location.
    /// </summary>
    [Fact]
    public void Update_NullLocation_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = null!,
            }
        );
        var ex = Assert.Throws<ArgumentException>(() => service.Update(o));
        Assert.Contains("location", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests Update with an invalid latitude.
    /// </summary>
    [Fact]
    public void Update_InvalidLatitude_ThrowsArgumentOutOfRangeException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 100, Longitude = 0 },
            }
        );
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update with an invalid longitude.
    /// </summary>
    [Fact]
    public void Update_InvalidLongitude_ThrowsArgumentOutOfRangeException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 0, Longitude = 200 },
            }
        );
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update when repository returns null (not found).
    /// </summary>
    [Fact]
    public void Update_NotFound_ThrowsKeyNotFoundException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo
            .Setup(r => r.Update(It.IsAny<Models.Occurrence>()))
            .Returns((Models.Occurrence)null!);

        Assert.Throws<KeyNotFoundException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update when repository throws an exception.
    /// </summary>
    [Fact]
    public void Update_RepositoryThrows_WrapsInvalidOperationException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo
            .Setup(r => r.Update(It.IsAny<Models.Occurrence>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.Update(o));
    }

    /// <summary>
    ///  Tests Update with valid input.
    /// </summary>
    [Fact]
    public void Update_Valid_ReturnsUpdatedOccurrence()
    {
        var updated = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 5,
                Title = "updated",
                Description = "d2",
                Type = OccurrenceType.FOREST_FIRE,
                Priority = PriorityLevel.HIGH,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 41, Longitude = -8.2 },
            }
        );
        mockRepo.Setup(r => r.Update(It.IsAny<Models.Occurrence>())).Returns(updated);

        var result = service.Update(updated);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("updated", result.Title);
    }

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
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 9,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo.Setup(r => r.Delete(9)).Returns(o);

        var result = service.Delete(9);

        Assert.NotNull(result);
        Assert.Equal(9, result.Id);
    }

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
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Id = 7,
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );
        mockRepo.Setup(r => r.GetOccurrenceById(7)).Returns(o);

        var result = service.GetOccurrenceById(7);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with invalid page number.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidPageNumber_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GetAllOccurrences(0, 10, "Title", "asc", "")
        );
        Assert.Contains("Page number", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with invalid page size.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidPageSize_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GetAllOccurrences(1, 0, "Title", "asc", "")
        );
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with too large page size.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_PageSizeTooLarge_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GetAllOccurrences(1, 1001, "Title", "asc", "")
        );
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with empty sort field.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_EmptySortBy_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GetAllOccurrences(1, 10, "", "asc", "")
        );
        Assert.Contains("Sort field", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences with invalid sort order.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_InvalidSortOrder_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GetAllOccurrences(1, 10, "Title", "invalid", "")
        );
        Assert.Contains("Sort order", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests GetAllOccurrences when repository returns null.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_RepositoryReturnsNull_ReturnsEmptyList()
    {
        mockRepo
            .Setup(r => r.GetAllOccurrences(1, 10, "Title", "asc", ""))
            .Returns((List<Occurrence>?)null!);

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
        mockRepo
            .Setup(r =>
                r.GetAllOccurrences(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() =>
            service.GetAllOccurrences(1, 10, "Title", "asc", "")
        );
    }

    /// <summary>
    ///  Tests GetAllOccurrences with valid parameters.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_Valid_ReturnsList()
    {
        var list = new List<Models.Occurrence>
        {
            new Occurrence(
                new OccurrenceCreateDto
                {
                    Id = 2,
                    Title = "Alpha",
                    Description = "d",
                    Type = OccurrenceType.FLOOD,
                    Priority = PriorityLevel.LOW,
                    ProximityRadius = 10,
                    Location = new GeoPoint { Latitude = 40, Longitude = -8 },
                }
            ),
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
            new Occurrence(
                new OccurrenceCreateDto
                {
                    Id = 1,
                    Title = "Flood",
                    Description = "d",
                    Type = OccurrenceType.FLOOD,
                    Priority = PriorityLevel.LOW,
                    ProximityRadius = 10,
                    Location = new GeoPoint { Latitude = 40, Longitude = -8 },
                }
            ),
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
            new Occurrence(
                new OccurrenceCreateDto
                {
                    Id = 3,
                    Title = "Active",
                    Description = "d",
                    Type = OccurrenceType.FOREST_FIRE,
                    Priority = PriorityLevel.HIGH,
                    ProximityRadius = 20,
                    Status = OccurrenceStatus.ACTIVE,
                    Location = new GeoPoint { Latitude = 41, Longitude = -8 },
                }
            ),
        };
        mockRepo.Setup(r => r.GetAllActiveOccurrences(1, 50, null, null, null)).Returns(list);

        var result = service.GetAllActiveOccurrences(1, 50, null, null, null);

        Assert.Single(result);
        Assert.All(
            result,
            o =>
                Assert.True(
                    o.Status == OccurrenceStatus.ACTIVE
                )
        );
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence with null occurrence.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_NullOccurrence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => service.CreateAdminOccurrence(null!));
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence with missing title.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_EmptyTitle_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "",
                Description = "desc",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        var ex = Assert.Throws<ArgumentException>(() => service.CreateAdminOccurrence(o));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence with missing description.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_EmptyDescription_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Test",
                Description = "",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        Assert.Throws<ArgumentException>(() => service.CreateAdminOccurrence(o));
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence with missing location.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_NullLocation_ThrowsArgumentException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Test",
                Description = "Desc",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 10,
                Location = null!,
            }
        );

        Assert.Throws<ArgumentException>(() => service.CreateAdminOccurrence(o));
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence sets ResponsibleEntityId based on coordinates.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_SetsResponsibleEntityId_WhenEntityExists()
    {
        var occurrence = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Incident",
                Description = "Desc",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        mockResponsibleEntityService
            .Setup(s => s.FindResponsibleEntity(OccurrenceType.FLOOD, 40, -8))
            .Returns(new ResponsibleEntity { Id = 99 });

        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Returns(
                (Occurrence o) =>
                {
                    o.Id = 10;
                    return o;
                }
            );

        var result = service.CreateAdminOccurrence(occurrence);

        Assert.Equal(10, result.Id);
        Assert.Equal(99, result.ResponsibleEntityId);
        Assert.Equal(0, result.ReportCount);
        Assert.Null(result.ReportId);
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence when no ResponsibleEntity is found.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_NoResponsibleEntity()
    {
        var occurrence = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Incident",
                Description = "Desc",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 41, Longitude = -8 },
            }
        );

        mockResponsibleEntityService
            .Setup(s =>
                s.FindResponsibleEntity(
                    It.IsAny<OccurrenceType>(),
                    It.IsAny<double>(),
                    It.IsAny<double>()
                )
            )
            .Returns((ResponsibleEntity?)null);

        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Returns(
                (Occurrence o) =>
                {
                    o.Id = 5;
                    return o;
                }
            );

        var result = service.CreateAdminOccurrence(occurrence);

        Assert.Equal(5, result.Id);
        Assert.Null(result.ResponsibleEntityId);
    }

    /// <summary>
    ///  Tests CreateAdminOccurrence wraps repository exceptions.
    /// </summary>
    [Fact]
    public void CreateAdminOccurrence_RepositoryThrows_WrapsInvalidOperationException()
    {
        var o = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "t",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                ProximityRadius = 10,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        mockRepo
            .Setup(r => r.Create(It.IsAny<Models.Occurrence>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => service.CreateAdminOccurrence(o));
    }

    /// <summary>
    /// Tests that Create occurrence computes proximity radius for types whose base priority is HIGH.
    /// </summary>
    [Fact]
    public void Create_ComputesProximityRadius_ForForestFire_High()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Forest fire",
                Description = "Smoke visible",
                Type = OccurrenceType.FOREST_FIRE,
                ProximityRadius = 0,
                ReportCount = 0,
                Location = new GeoPoint { Latitude = 40.0, Longitude = -8.0 },
            }
        );

        Occurrence? saved = null;
        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Callback<Occurrence>(o => saved = o)
            .Returns<Occurrence>(o =>
            {
                o.Id = 42;
                return o;
            });

        var result = service.Create(input);

        Assert.NotNull(saved);
        Assert.Equal(PriorityLevel.HIGH, saved!.Priority);
        Assert.Equal(2500.0 * 2.0, saved.ProximityRadius, 6);
        Assert.Equal(42, result.Id);
    }

    /// <summary>
    /// Tests that Create occurrence computes proximity radius for types whose base priority is MEDIUM.
    /// </summary>
    [Fact]
    public void Create_ComputesProximityRadius_ForRoadObstruction_Medium()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Road obstruction",
                Description = "Debris on road",
                Type = OccurrenceType.ROAD_OBSTRUCTION,
                ProximityRadius = 0,
                ReportCount = 0,
                Location = new GeoPoint { Latitude = 40.5, Longitude = -8.5 },
            }
        );

        Occurrence? saved = null;
        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Callback<Occurrence>(o => saved = o)
            .Returns<Occurrence>(o =>
            {
                o.Id = 43;
                return o;
            });

        var result = service.Create(input);

        Assert.NotNull(saved);
        Assert.Equal(PriorityLevel.MEDIUM, saved!.Priority);
        Assert.Equal(200.0 * 1.5, saved.ProximityRadius, 6);
        Assert.Equal(43, result.Id);
    }

    /// <summary>
    /// Tests that Create occurrence computes proximity radius for types whose base priority is LOW.
    /// </summary>
    [Fact]
    public void Create_ComputesProximityRadius_ForPublicLighting_Low()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Street light out",
                Description = "Lamp not working",
                Type = OccurrenceType.PUBLIC_LIGHTING,
                ProximityRadius = 0,
                ReportCount = 0,
                Location = new GeoPoint { Latitude = 41.2, Longitude = -8.3 },
            }
        );

        Occurrence? saved = null;
        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Callback<Occurrence>(o => saved = o)
            .Returns<Occurrence>(o =>
            {
                o.Id = 44;
                return o;
            });

        var result = service.Create(input);

        Assert.NotNull(saved);
        Assert.Equal(PriorityLevel.LOW, saved!.Priority);
        Assert.Equal(100.0 * 1.0, saved.ProximityRadius, 6);
        Assert.Equal(44, result.Id);
    }

    /// <summary>
    /// Tests that Create occurrence computes priority and resulting proximity radius when reportCount raises priority.
    /// </summary>
    [Fact]
    public void Create_ComputesPriorityAndRadius_WhenReportCountElevatesPriority()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "Vehicle breakdown cluster",
                Description = "Multiple reports",
                Type = OccurrenceType.VEHICLE_BREAKDOWN,
                ProximityRadius = 0,
                ReportCount = 5,
                Location = new GeoPoint { Latitude = 41.0, Longitude = -8.5 },
            }
        );

        Occurrence? saved = null;
        mockRepo
            .Setup(r => r.Create(It.IsAny<Occurrence>()))
            .Callback<Occurrence>(o => saved = o)
            .Returns<Occurrence>(o =>
            {
                o.Id = 99;
                return o;
            });

        var result = service.Create(input);

        Assert.NotNull(saved);
        Assert.Equal(PriorityLevel.HIGH, saved!.Priority);
        Assert.Equal(125.0 * 2.0, saved.ProximityRadius, 6);
        Assert.Equal(99, result.Id);
    }

    /// Tests Create with a Forest Fire occurrence sets priority to HIGH.
    /// </summary>
    [Fact]
    public void Create_ForestFire_ShouldSetPriorityHigh()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "test",
                Description = "test",
                Type = OccurrenceType.FOREST_FIRE,
                ReportCount = 0,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        mockRepo.Setup(r => r.Create(It.IsAny<Occurrence>())).Returns((Occurrence o) => o);

        var result = service.Create(input);

        Assert.Equal(PriorityLevel.HIGH, result.Priority);
    }

    /// <summary>
    /// Tests Create with a Pollution occurrence and low report count keeps priority MEDIUM.
    /// </summary>
    [Fact]
    public void Create_Pollution_WithLowReportCount_ShouldRemainMediumPriority()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "test",
                Description = "test",
                Type = OccurrenceType.POLLUTION,
                ReportCount = 2,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 41, Longitude = -7 },
            }
        );

        mockRepo.Setup(r => r.Create(It.IsAny<Occurrence>())).Returns((Occurrence o) => o);

        var result = service.Create(input);

        Assert.Equal(PriorityLevel.MEDIUM, result.Priority);
    }

    /// <summary>
    /// Tests Create with a Traffic Congestion occurrence and high report count increases priority to HIGH.
    /// </summary>
    [Fact]
    public void Create_TrafficCongestion_WithHighReports_ShouldIncreasePriorityToHigh()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "test",
                Description = "test",
                Type = OccurrenceType.TRAFFIC_CONGESTION,
                ReportCount = 6,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        mockRepo.Setup(r => r.Create(It.IsAny<Occurrence>())).Returns((Occurrence o) => o);

        var result = service.Create(input);

        Assert.Equal(PriorityLevel.HIGH, result.Priority);
    }

    /// <summary>
    /// Tests Create with a Public Lighting occurrence and low report count keeps priority LOW.
    /// </summary>
    [Fact]
    public void Create_PublicLighting_WithLowReportCount_ShouldRemainLowPriority()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "test",
                Description = "test",
                Type = OccurrenceType.PUBLIC_LIGHTING,
                ReportCount = 3,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 39, Longitude = -9 },
            }
        );

        mockRepo.Setup(r => r.Create(It.IsAny<Occurrence>())).Returns((Occurrence o) => o);

        var result = service.Create(input);

        Assert.Equal(PriorityLevel.LOW, result.Priority);
    }

    /// <summary>
    /// Tests Create with a Lost Animal occurrence and moderate report count sets priority to MEDIUM.
    /// </summary>
    [Fact]
    public void Create_LostAnimal_WithReportCountAboveThreshold_ShouldSetPriorityMedium()
    {
        var input = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = "test",
                Description = "test",
                Type = OccurrenceType.LOST_ANIMAL,
                ReportCount = 7,
                ProximityRadius = 50,
                Location = new GeoPoint { Latitude = 40, Longitude = -8 },
            }
        );

        mockRepo.Setup(r => r.Create(It.IsAny<Occurrence>())).Returns((Occurrence o) => o);

        var result = service.Create(input);

        Assert.Equal(PriorityLevel.MEDIUM, result.Priority);
    }
}

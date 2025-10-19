using GeoPointModel = readytohelpapi.GeoPoint.Models.GeoPoint;
using readytohelpapi.Occurrence.Data;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

/// <summary>
///  This class contains unit tests for the OccurrenceRepository.
/// </summary>
public class TestOccurrenceRepositoryTest : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly OccurrenceContext _occurrenceContext;
    private readonly IOccurrenceRepository _occurrenceRepository;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestOccurrenceRepositoryTest"/> class.
    /// </summary>
    public TestOccurrenceRepositoryTest(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        _occurrenceContext = this.fixture.Context;
        _occurrenceRepository = new OccurrenceRepository(_occurrenceContext);
    }

    /// <summary>
    /// Tests that a valid occurrence can be created successfully.
    /// </summary>
    [Fact]
    public void Create_ValidOccurrence_ReturnsCreatedOccurrence()
    {
        var o = new Models.Occurrence
        {
            Title = "Forest Fire",
            Description = "Near hills",
            Type = OccurrenceType.FOREST_FIRE,
            Priority = PriorityLevel.HIGH,
            ProximityRadius = 500,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40.123, Longitude = -8.456 }
        };

        var created = _occurrenceRepository.Create(o);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Forest Fire", created.Title);
    }

    /// <summary>
    /// Tests that creating a null occurrence throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Create_NullOccurrence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _occurrenceRepository.Create(null!));
    }

    /// <summary>
    /// Tests retrieving an occurrence by ID when it exists.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_ShouldReturnOccurrence_WhenExists()
    {
        var o = new Models.Occurrence
        {
            Title = "Flooded Street",
            Description = "Water rising",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 100,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40.1, Longitude = -8.4 }
        };
        _occurrenceContext.Occurrences.Add(o);
        _occurrenceContext.SaveChanges();

        var got = _occurrenceRepository.GetOccurrenceById(o.Id);

        Assert.NotNull(got);
        Assert.Equal(o.Id, got!.Id);
    }

    /// <summary>
    /// Tests that retrieving an occurrence by a non-existing ID returns null.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_ShouldReturnNull_WhenNotExists()
    {
        var got = _occurrenceRepository.GetOccurrenceById(987654);
        Assert.Null(got);
    }

    /// <summary>
    /// Tests that retrieving by ID with zero returns null.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_WithZeroId_ReturnsNull()
    {
        var got = _occurrenceRepository.GetOccurrenceById(0);
        Assert.Null(got);
    }

    /// <summary>
    /// Tests that retrieving by ID with a negative value returns null.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_WithNegativeId_ReturnsNull()
    {
        var got = _occurrenceRepository.GetOccurrenceById(-10);
        Assert.Null(got);
    }

    /// <summary>
    /// Tests retrieving occurrences by title when partial matches exist.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_ShouldReturnList_WhenPartialMatch()
    {
        var o1 = new Models.Occurrence
        {
            Title = "Road Obstruction",
            Description = "desc",
            Type = OccurrenceType.ROAD_OBSTRUCTION,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 5,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40.2, Longitude = -8.5 }
        };
        var o2 = new Models.Occurrence
        {
            Title = "Broadway Event",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 5,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40.3, Longitude = -8.6 }
        };
        _occurrenceContext.Occurrences.AddRange(o1, o2);
        _occurrenceContext.SaveChanges();

        var list = _occurrenceRepository.GetOccurrenceByTitle("road");

        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.Title.Contains("Road", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tests that retrieving by title with null returns an empty list.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_WithNull_ReturnsEmptyList()
    {
        var list = _occurrenceRepository.GetOccurrenceByTitle(null!);
        Assert.Empty(list);
    }

    /// <summary>
    /// Tests that retrieving by title with an empty string returns an empty list.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_WithEmptyString_ReturnsEmptyList()
    {
        var list = _occurrenceRepository.GetOccurrenceByTitle(string.Empty);
        Assert.Empty(list);
    }

    /// <summary>
    /// Tests that retrieving by title with whitespace returns an empty list.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_WithWhitespace_ReturnsEmptyList()
    {
        var list = _occurrenceRepository.GetOccurrenceByTitle("   ");
        Assert.Empty(list);
    }

    /// <summary>
    /// Tests that title search is case-insensitive.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_CaseInsensitive_ReturnsMatches()
    {
        var o = new Models.Occurrence
        {
            Title = "CaseSensitiveTitle",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 5,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 41, Longitude = -8 }
        };
        _occurrenceContext.Occurrences.Add(o);
        _occurrenceContext.SaveChanges();

        var list = _occurrenceRepository.GetOccurrenceByTitle("casesensitivetitle");

        Assert.NotEmpty(list);
        Assert.Contains(list, occ => occ.Title.Equals("CaseSensitiveTitle", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests filtered, sorted, and paginated retrieval combining filter on title and description.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_ShouldFilterByTitleOrDescription()
    {
        _occurrenceContext.Occurrences.AddRange(
            new Models.Occurrence
            {
                Title = "Alpha",
                Description = "foo",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0, Longitude = -8.0 }
            },
            new Models.Occurrence
            {
                Title = "Beta",
                Description = "bar needle",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.HIGH,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0, Longitude = -8.1 }
            },
            new Models.Occurrence
            {
                Title = "Gamma needle",
                Description = "baz",
                Type = OccurrenceType.FOREST_FIRE,
                Priority = PriorityLevel.MEDIUM,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0, Longitude = -8.2 }
            }
        );
        _occurrenceContext.SaveChanges();

        var page = _occurrenceRepository.GetAllOccurrences(1, 10, "title", "asc", "needle");

        Assert.Equal(2, page.Count);
        Assert.All(page, o => Assert.True(
            o.Title.Contains("needle", StringComparison.OrdinalIgnoreCase) ||
            o.Description.Contains("needle", StringComparison.OrdinalIgnoreCase)
        ));
    }

    /// <summary>
    /// Tests sorting by title descending.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_ShouldSortByTitleDesc()
    {
        _occurrenceContext.Occurrences.AddRange(
            new Models.Occurrence
            {
                Title = "Alpha",
                Description = "a",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0, Longitude = -8.0 }
            },
            new Models.Occurrence
            {
                Title = "Zulu",
                Description = "z",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.HIGH,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0, Longitude = -8.0 }
            }
        );
        _occurrenceContext.SaveChanges();

        var page = _occurrenceRepository.GetAllOccurrences(1, 10, "title", "desc", string.Empty);

        Assert.True(page.Count >= 2);
        Assert.Equal("Zulu", page.First().Title);
    }

    /// <summary>
    /// Tests that unknown sortBy defaults to sorting by ID.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_UnknownSortBy_DefaultsToId()
    {
        var a = new Models.Occurrence
        {
            Title = "A",
            Description = "a",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 39.9, Longitude = -8.0 }
        };
        var b = new Models.Occurrence
        {
            Title = "B",
            Description = "b",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 39.9, Longitude = -8.1 }
        };
        _occurrenceContext.Occurrences.AddRange(a, b);
        _occurrenceContext.SaveChanges();

        var asc = _occurrenceRepository.GetAllOccurrences(1, 10, "unknown", "asc", string.Empty);
        var desc = _occurrenceRepository.GetAllOccurrences(1, 10, "unknown", "desc", string.Empty);

        Assert.True(asc.First().Id <= asc.Last().Id);
        Assert.True(desc.First().Id >= desc.Last().Id);
        Assert.Equal(a.Id, asc.First().Id);
        Assert.Equal(b.Id, desc.First().Id);
    }

    /// <summary>
    /// Tests pagination parameters and clamping behavior for pageNumber and pageSize.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_Pagination_Works_AndClamps()
    {
        for (int i = 0; i < 15; i++)
        {
            _occurrenceContext.Occurrences.Add(new Models.Occurrence
            {
                Title = $"Item {i:D2}",
                Description = "x",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 1,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40.0 + i * 0.001, Longitude = -8.0 }
            });
        }
        _occurrenceContext.SaveChanges();

        var defaulted = _occurrenceRepository.GetAllOccurrences(0, 0, "title", "asc", string.Empty);
        Assert.Equal(10, defaulted.Count);

        var page2 = _occurrenceRepository.GetAllOccurrences(2, 10, "title", "asc", string.Empty);
        Assert.True(page2.Count <= 10);
        Assert.True(page2.Count >= 1);
    }

    /// <summary>
    /// Tests retrieving occurrences by a specific type.
    /// </summary>
    [Fact]
    public void GetOccurrencesByType_ReturnsMatchingType()
    {
        _occurrenceContext.Occurrences.AddRange(
            new Models.Occurrence
            {
                Title = "Flood A",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
            },
            new Models.Occurrence
            {
                Title = "Fire B",
                Description = "d",
                Type = OccurrenceType.FOREST_FIRE,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 41, Longitude = -8 }
            }
        );
        _occurrenceContext.SaveChanges();

        var floods = _occurrenceRepository.GetOccurrencesByType(OccurrenceType.FLOOD);

        Assert.NotEmpty(floods);
        Assert.All(floods, o => Assert.Equal(OccurrenceType.FLOOD, o.Type));
    }

    /// <summary>
    /// Tests retrieving occurrences by a specific status.
    /// </summary>
    [Fact]
    public void GetOccurrencesByStatus_ReturnsMatchingStatus()
    {
        _occurrenceContext.Occurrences.Add(new Models.Occurrence
        {
            Title = "Active 1",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        });
        _occurrenceContext.SaveChanges();

        var actives = _occurrenceRepository.GetOccurrencesByStatus(OccurrenceStatus.ACTIVE);

        Assert.NotEmpty(actives);
        Assert.All(actives, o => Assert.Equal(OccurrenceStatus.ACTIVE, o.Status));
    }

    /// <summary>
    /// Tests retrieving all active occurrences.
    /// </summary>
    [Fact]
    public void GetAllActiveOccurrences_ReturnsActiveOnly()
    {
        _occurrenceContext.Occurrences.Add(new Models.Occurrence
        {
            Title = "Active X",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        });
        _occurrenceContext.SaveChanges();

        var list = _occurrenceRepository.GetAllActiveOccurrences();

        Assert.NotEmpty(list);
        Assert.All(list, o => Assert.Equal(OccurrenceStatus.ACTIVE, o.Status));
    }

    /// <summary>
    /// Tests retrieving occurrences by a specific priority.
    /// </summary>
    [Fact]
    public void GetOccurrencesByPriority_ReturnsMatchingPriority()
    {
        _occurrenceContext.Occurrences.AddRange(
            new Models.Occurrence
            {
                Title = "Low P",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.LOW,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
            },
            new Models.Occurrence
            {
                Title = "High P",
                Description = "d",
                Type = OccurrenceType.FLOOD,
                Priority = PriorityLevel.HIGH,
                ProximityRadius = 10,
                Status = OccurrenceStatus.ACTIVE,
                Location = new GeoPointModel { Latitude = 41, Longitude = -8 }
            }
        );
        _occurrenceContext.SaveChanges();

        var highs = _occurrenceRepository.GetOccurrencesByPriority(PriorityLevel.HIGH);

        Assert.NotEmpty(highs);
        Assert.All(highs, o => Assert.Equal(PriorityLevel.HIGH, o.Priority));
    }

    /// <summary>
    /// Tests retrieving an occurrence by reportId when it exists.
    /// </summary>
    [Fact]
    public void GetByReportId_ShouldReturnOccurrence_WhenExists()
    {
        var created = new Models.Occurrence
        {
            Title = "Report Ref",
            Description = "d",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            ReportId = 12345,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        _occurrenceContext.Occurrences.Add(created);
        _occurrenceContext.SaveChanges();

        var got = _occurrenceRepository.GetByReportId(12345);

        Assert.NotNull(got);
        Assert.Equal(created.Id, got!.Id);
    }

    /// <summary>
    /// Tests that retrieving by a non-existing reportId returns null.
    /// </summary>
    [Fact]
    public void GetByReportId_ShouldReturnNull_WhenNotExists()
    {
        var got = _occurrenceRepository.GetByReportId(999999);
        Assert.Null(got);
    }

    /// <summary>
    /// Tests that retrieving by an invalid reportId (zero or negative) returns null.
    /// </summary>
    [Fact]
    public void GetByReportId_WithInvalidId_ReturnsNull()
    {
        Assert.Null(_occurrenceRepository.GetByReportId(0));
        Assert.Null(_occurrenceRepository.GetByReportId(-10));
    }

    /// <summary>
    /// Tests that an existing occurrence can be updated successfully.
    /// </summary>
    [Fact]
    public void Update_ExistingOccurrence_ReturnsUpdatedOccurrence()
    {
        var o = new Models.Occurrence
        {
            Title = "Old",
            Description = "OldD",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        _occurrenceContext.Occurrences.Add(o);
        _occurrenceContext.SaveChanges();

        o.Title = "New";
        o.Priority = PriorityLevel.HIGH;

        var updated = _occurrenceRepository.Update(o);

        Assert.NotNull(updated);
        Assert.Equal("New", updated.Title);
        Assert.Equal(PriorityLevel.HIGH, updated.Priority);
    }

    /// <summary>
    /// Tests that updating a null occurrence throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Update_NullOccurrence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _occurrenceRepository.Update(null!));
    }

    /// <summary>
    /// Tests updating a non-existing occurrence: repository currently does not throw; entity remains not persisted.
    /// </summary>
    [Fact]
    public void Update_NonExistingOccurrence_ThrowsDbUpdateException()
    {
        var occurrence = new Models.Occurrence
        {
            Id = 987654,
            Title = "Ghost",
            Description = "NoRow",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 1,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 39.5, Longitude = -8.5 }
        };

        Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(() =>
            _occurrenceRepository.Update(occurrence)
        );
    }

    /// <summary>
    /// Tests that deleting an existing occurrence removes and returns it.
    /// </summary>
    [Fact]
    public void Delete_ExistingOccurrence_ReturnsDeletedOccurrence()
    {
        var o = new Models.Occurrence
        {
            Title = "Del",
            Description = "D",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            Status = OccurrenceStatus.ACTIVE,
            Location = new GeoPointModel { Latitude = 40, Longitude = -8 }
        };
        _occurrenceContext.Occurrences.Add(o);
        _occurrenceContext.SaveChanges();

        var deleted = _occurrenceRepository.Delete(o.Id);

        Assert.NotNull(deleted);
        Assert.Null(_occurrenceRepository.GetOccurrenceById(o.Id));
    }

    /// <summary>
    /// Tests that deleting a non-existing occurrence returns null.
    /// </summary>
    [Fact]
    public void Delete_NonExisting_ReturnsNull()
    {
        var deleted = _occurrenceRepository.Delete(555555);
        Assert.Null(deleted);
    }

    /// <summary>
    /// Tests the default constructor sets sensible defaults (CreationDateTime, Status, ReportCount).
    /// </summary>
    [Fact]
    public void Model_DefaultConstructor_SetsDefaults()
    {
        var now = DateTime.UtcNow.AddSeconds(-5);
        var o = new Models.Occurrence();

        Assert.Equal(OccurrenceStatus.ACTIVE, o.Status);
        Assert.Equal(0, o.ReportCount);
        Assert.True(o.CreationDateTime >= now);
    }
}
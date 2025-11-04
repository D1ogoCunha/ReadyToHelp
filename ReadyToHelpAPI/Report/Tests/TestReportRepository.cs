namespace readytohelpapi.Report.Tests;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

/// <summary>
/// This class contains all integration tests related to ReportRepository.
/// </summary>
[Trait("Category", "Integration")]
public class TestReportRepository : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext ctx;
    private readonly IReportRepository repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestReportRepository"/> class.
    /// </summary>
    /// <param name="fixture">The database fixture.</param>
    public TestReportRepository(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        ctx = this.fixture.Context;
        repo = new ReportRepository(ctx);
    }

    /// <summary>
    /// Tests the Create method with a valid report.
    /// </summary>
    [Fact]
    public void Create_Valid_ReturnsCreated()
    {
        var user = UserFixture.CreateOrUpdateUser(email: $"test-{Guid.NewGuid():N}@example.com");
        ctx.Users.Add(user);
        ctx.SaveChanges();

        var report = ReportFixture.CreateOrUpdate(
            title: "Buraco",
            description: "Buraco na estrada",
            userId: user.Id,
            type: OccurrenceType.ROAD_DAMAGE,
            location: new GeoPoint { Latitude = 41.15, Longitude = -8.61 }
        );

        var created = repo.Create(report);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Buraco", created.Title);
    }

    /// <summary>
    /// Tests the Create method with a null report.
    /// </summary>
    [Fact]
    public void Create_Null_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() => repo.Create(null!));
    }

    [Fact]
    public void GetById_WhenExists_ReturnsEntity()
    {
        var user = UserFixture.CreateOrUpdateUser(email: $"test-{Guid.NewGuid():N}@example.com");
        ctx.Users.Add(user);
        ctx.SaveChanges();

        var toAdd = ReportFixture.CreateOrUpdate(title: "Teste Get", userId: user.Id);
        ctx.Reports.Add(toAdd);
        ctx.SaveChanges();

        var found = repo.GetById(toAdd.Id);

        Assert.NotNull(found);
        Assert.Equal(toAdd.Id, found!.Id);
        Assert.Equal("Teste Get", found.Title);
    }

    /// <summary>
    /// Tests the GetById method when the report does not exist.
    /// </summary>
    [Fact]
    public void GetById_WhenNotExists_ReturnsNull()
    {
        var found = repo.GetById(999999);
        Assert.Null(found);
    }

    /// <summary>
    /// Tests the GetById method with invalid IDs (zero and negative).
    /// </summary>
    [Fact]
    public void GetById_InvalidId_ReturnsNull_ForZeroAndNegative()
    {
        Assert.Null(repo.GetById(0));
        Assert.Null(repo.GetById(-10));
    }

    /// <summary>
    /// Tests creating a report with a duplicate ID throws a DbUpdateException.
    /// </summary>
    [Fact]
    public void Create_DuplicateId_ThrowsDbUpdateException()
    {
        var user = UserFixture.CreateOrUpdateUser(email: $"dup-{Guid.NewGuid():N}@example.com");
        ctx.Users.Add(user);
        ctx.SaveChanges();

        var report1 = ReportFixture.CreateOrUpdate(
            id: 999999,
            title: "Dup1",
            description: "first",
            userId: user.Id
        );

        var created1 = repo.Create(report1);
        Assert.NotNull(created1);

        var report2 = ReportFixture.CreateOrUpdate(
            id: created1.Id,
            title: "Dup2",
            description: "second",
            userId: user.Id
        );

        var ex = Assert.Throws<DbUpdateException>(() => repo.Create(report2));
        Assert.NotNull(ex.InnerException);
        Assert.True(ex.InnerException.Message.Length > 0);
    }
}

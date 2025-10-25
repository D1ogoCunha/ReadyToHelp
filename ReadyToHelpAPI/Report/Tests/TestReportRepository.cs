namespace readytohelpapi.Report.Tests;

using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.GeoPoint.Models;
using Xunit;
using readytohelpapi.Common.Data;
using readytohelpapi.User.Models;
using readytohelpapi.User.Tests.Fixtures;

public class TestReportRepository : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext ctx;
    private readonly IReportRepository repo;

    public TestReportRepository(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        ctx = this.fixture.Context;
        repo = new ReportRepository(ctx);
    }

    [Fact]
    public void Create_Valid_ReturnsCreated()
    {
        var user = UserFixture.CreateOrUpdateUser(
            email: $"test-{Guid.NewGuid():N}@example.com"
        );
        ctx.Users.Add(user);
        ctx.SaveChanges();

        var report = ReportFixture.CreateOrUpdate(
            title: "Buraco",
            description: "Buraco na estrada",
            userId: user.Id,
            type: OccurrenceType.ROAD_DAMAGE,
            priority: PriorityLevel.LOW,
            location: new GeoPoint { Latitude = 41.15, Longitude = -8.61 }
        );

        var created = repo.Create(report);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Buraco", created.Title);
    }

    [Fact]
    public void Create_Null_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() => repo.Create(null!));
    }

    [Fact]
    public void GetById_WhenExists_ReturnsEntity()
    {
        var user = UserFixture.CreateOrUpdateUser(
            email: $"test-{Guid.NewGuid():N}@example.com"
        );
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

    [Fact]
    public void GetById_WhenNotExists_ReturnsNull()
    {
        var found = repo.GetById(999999);
        Assert.Null(found);
    }
}
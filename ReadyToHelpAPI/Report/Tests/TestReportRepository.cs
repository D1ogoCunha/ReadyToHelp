namespace readytohelpapi.Report.Tests;

using readytohelpapi.Report.Data;
using readytohelpapi.Report.Models;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.GeoPoint.Models;
using Xunit;


public class TestReportRepository : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly ReportContext ctx;
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
        var report = ReportFixture.CreateOrUpdate(
            title: "Buraco",
            description: "Buraco na estrada",
            userId: 123,
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
        var toAdd = ReportFixture.CreateOrUpdate(title: "Teste Get", userId: 1);
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
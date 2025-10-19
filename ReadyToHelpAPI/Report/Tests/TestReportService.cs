namespace readytohelpapi.Report.Tests;

using Moq;
using Xunit;
using ReportModel = readytohelpapi.Report.Models.Report;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.GeoPoint.Models;

public class TestReportService
{
    private readonly Mock<IReportRepository> mockRepo = new();
    private readonly Mock<IOccurrenceService> mockOccSvc = new();
    private readonly IReportService service;

    public TestReportService()
    {
        service = new ReportServiceImpl(mockRepo.Object, mockOccSvc.Object);
    }

    private static GeoPoint Pt(double lat = 41.3678, double lon = -8.2012) => new GeoPoint { Latitude = lat, Longitude = lon };

    [Fact]
    public void Create_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => service.Create(null!));
    }

    [Fact]
    public void Create_InvalidTitle_Throws()
    {
        var r = ReportFixture.CreateOrUpdate(title: "", description: "d", userId: 1, location: Pt());
        Assert.Throws<ArgumentException>(() => service.Create(r));
    }

    [Fact]
    public void Create_InvalidDescription_Throws()
    {
        var r = ReportFixture.CreateOrUpdate(title: "t", description: "", userId: 1, location: Pt());
        Assert.Throws<ArgumentException>(() => service.Create(r));
    }

    [Fact]
    public void Create_InvalidUserId_Throws()
    {
        var r = ReportFixture.CreateOrUpdate(title: "t", description: "d", userId: 0, location: Pt());
        Assert.Throws<ArgumentException>(() => service.Create(r));
    }

    [Fact]
    public void Create_MissingLocation_Throws()
    {
        var r = new ReportModel
        {
            Title = "t",
            Description = "d",
            UserId = 1,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = null
        };

        Assert.Throws<ArgumentException>(() => service.Create(r));
    }

    [Fact]
    public void Create_NoNearbyOccurrence_CreatesOccurrence()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", description: "desc", userId: 1, location: Pt());
        var createdReport = ReportFixture.CreateOrUpdate(id: 100, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);

        mockOccSvc.Setup(s => s.GetOccurrencesByType(input.Type)).Returns(new List<Occurrence>());
        mockRepo.Setup(r => r.Create(It.IsAny<ReportModel>())).Returns(createdReport);

        var createdOccurrence = new Occurrence { Id = 200 };
        mockOccSvc.Setup(s => s.Create(It.IsAny<Occurrence>())).Returns(createdOccurrence);

        var (rep, occ) = service.Create(input);

        Assert.Equal(100, rep.Id);
        Assert.Equal(200, occ.Id);

        mockOccSvc.Verify(s => s.Update(It.IsAny<Occurrence>()), Times.Never);
        mockOccSvc.Verify(s => s.Create(It.Is<Occurrence>(o =>
            o.Title == input.Title &&
            o.Description == input.Description &&
            o.Type == input.Type &&
            o.Priority == input.Priority &&
            o.ProximityRadius == 50 &&
            o.Status == OccurrenceStatus.WAITING &&
            o.ReportCount == 1 &&
            o.ReportId == 100 &&
            o.Location != null &&
            Math.Abs(o.Location!.Latitude - input.Location!.Latitude) < 1e-9 &&
            Math.Abs(o.Location!.Longitude - input.Location!.Longitude) < 1e-9
        )), Times.Once);
    }

    [Fact]
    public void Create_DuplicateWithin50m_IncrementsCount_StaysWaiting()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", userId: 1, location: Pt(41.3678, -8.2012));
        var createdReport = ReportFixture.CreateOrUpdate(id: 123, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);

        var existingOcc = new Occurrence
        {
            Id = 10,
            ReportId = 999, // âncora
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = input.Type
        };

        mockOccSvc.Setup(s => s.GetOccurrencesByType(input.Type)).Returns(new List<Occurrence> { existingOcc });

        // Âncora com mesma localização (distância 0 <= 50)
        var anchorReport = ReportFixture.CreateOrUpdate(id: 999, title: "anchor", description: "a", userId: 1, location: Pt(41.3678, -8.2012));
        mockRepo.Setup(r => r.GetById(999)).Returns(anchorReport);

        mockRepo.Setup(r => r.Create(It.IsAny<ReportModel>())).Returns(createdReport);
        mockOccSvc.Setup(s => s.Update(It.IsAny<Occurrence>())).Returns<Occurrence>(o => o);

        var (rep, occ) = service.Create(input);

        Assert.Equal(123, rep.Id);
        Assert.Equal(2, occ.ReportCount);
        Assert.Equal(OccurrenceStatus.WAITING, occ.Status);

        mockOccSvc.Verify(s => s.Update(It.Is<Occurrence>(o => o.Id == existingOcc.Id && o.ReportCount == 2)), Times.Once);
        mockOccSvc.Verify(s => s.Create(It.IsAny<Occurrence>()), Times.Never);
    }

    [Fact]
    public void Create_Duplicate_Reaches3_ActivatesOccurrence()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", userId: 1, location: Pt());
        var createdReport = ReportFixture.CreateOrUpdate(id: 555, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);

        var existingOcc = new Occurrence
        {
            Id = 22,
            ReportId = 1000,
            ReportCount = 2, // vai para 3
            Status = OccurrenceStatus.WAITING,
            Type = input.Type
        };

        mockOccSvc.Setup(s => s.GetOccurrencesByType(input.Type)).Returns(new List<Occurrence> { existingOcc });

        var anchorReport = ReportFixture.CreateOrUpdate(id: 1000, location: Pt()); // mesma localização
        mockRepo.Setup(r => r.GetById(1000)).Returns(anchorReport);

        mockRepo.Setup(r => r.Create(It.IsAny<ReportModel>())).Returns(createdReport);
        mockOccSvc.Setup(s => s.Update(It.IsAny<Occurrence>())).Returns<Occurrence>(o => o);

        var (rep, occ) = service.Create(input);

        Assert.Equal(3, occ.ReportCount);
        Assert.Equal(OccurrenceStatus.ACTIVE, occ.Status);
        mockOccSvc.Verify(s => s.Update(It.Is<Occurrence>(o => o.Id == 22 && o.Status == OccurrenceStatus.ACTIVE && o.ReportCount == 3)), Times.Once);
        mockOccSvc.Verify(s => s.Create(It.IsAny<Occurrence>()), Times.Never);
    }

    [Fact]
    public void Create_Duplicate_AlreadyActive_KeepsActive()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", userId: 1, location: Pt());
        var createdReport = ReportFixture.CreateOrUpdate(id: 777, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);

        var existingOcc = new Occurrence
        {
            Id = 33,
            ReportId = 2000,
            ReportCount = 5,
            Status = OccurrenceStatus.ACTIVE,
            Type = input.Type
        };

        mockOccSvc.Setup(s => s.GetOccurrencesByType(input.Type)).Returns(new List<Occurrence> { existingOcc });
        var anchorReport = ReportFixture.CreateOrUpdate(id: 2000, location: Pt());
        mockRepo.Setup(r => r.GetById(2000)).Returns(anchorReport);

        mockRepo.Setup(r => r.Create(It.IsAny<ReportModel>())).Returns(createdReport);
        mockOccSvc.Setup(s => s.Update(It.IsAny<Occurrence>())).Returns<Occurrence>(o => o);

        var (_, occ) = service.Create(input);

        Assert.Equal(6, occ.ReportCount);
        Assert.Equal(OccurrenceStatus.ACTIVE, occ.Status);
        mockOccSvc.Verify(s => s.Update(It.Is<Occurrence>(o => o.Id == 33 && o.ReportCount == 6 && o.Status == OccurrenceStatus.ACTIVE)), Times.Once);
    }
}
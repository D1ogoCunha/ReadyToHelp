namespace readytohelpapi.Report.Tests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ReportModel = readytohelpapi.Report.Models.Report;
using readytohelpapi.Report.Controllers;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.GeoPoint.Models;

public class TestReportApiController : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly Mock<IReportService> mockReportService;
    private readonly Mock<IReportRepository> mockReportRepository;
    private readonly ReportApiController controller;

    public TestReportApiController(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();

        mockReportService = new Mock<IReportService>();
        mockReportRepository = new Mock<IReportRepository>();
        controller = new ReportApiController(mockReportService.Object, mockReportRepository.Object);
    }

    private static GeoPoint Pt() => new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 };

    [Fact]
    public void Create_NullReport_ReturnsBadRequest()
    {
        var result = controller.Create(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mockReportService.Verify(s => s.Create(It.IsAny<ReportModel>()), Times.Never);
    }

    [Fact]
    public void Create_Valid_ReturnsCreatedAtAction_WithResponseObject()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", description: "desc", userId: 1, location: Pt());
        var createdReport = ReportFixture.CreateOrUpdate(id: 101, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);
        var createdOccurrence = new Occurrence
        {
            Id = 202,
            ReportId = 101,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = input.Type,
            Priority = input.Priority,
            Location = Pt()
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
                         .Returns((createdReport, createdOccurrence));

        var result = controller.Create(input);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);

        var value = created.Value!;
        var t = value.GetType();
        Assert.Equal(101, (int)t.GetProperty("reportId")!.GetValue(value)!);
        Assert.Equal(202, (int)t.GetProperty("occurrenceId")!.GetValue(value)!);

        mockReportService.Verify(s => s.Create(It.Is<ReportModel>(r =>
            r.Title == input.Title &&
            r.Description == input.Description &&
            r.UserId == input.UserId &&
            r.Location != null
        )), Times.Once);
    }

    [Fact]
    public void Create_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var input = ReportFixture.CreateOrUpdate(title: "X", description: "Y", userId: 1, location: Pt());
        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
                         .Throws(new ArgumentException("invalid"));

        var result = controller.Create(input);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var input = ReportFixture.CreateOrUpdate(title: "X", description: "Y", userId: 1, location: Pt());
        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
                         .Throws(new Exception("fail"));

        var result = controller.Create(input);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

        [Fact]
    public void GetById_InvalidId_ReturnsBadRequest()
    {
        var result = controller.GetById(0);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid id.", bad.Value);
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetById_NotFound_ReturnsNotFound()
    {
        mockReportRepository.Setup(r => r.GetById(42)).Returns((ReportModel?)null);

        var result = controller.GetById(42);

        Assert.IsType<NotFoundResult>(result);
        mockReportRepository.Verify(r => r.GetById(42), Times.Once);
    }

    [Fact]
    public void GetById_Found_ReturnsOkWithReport()
    {
        var rep = ReportFixture.CreateOrUpdate(id: 7, title: "Ok", userId: 1, location: Pt());
        mockReportRepository.Setup(r => r.GetById(7)).Returns(rep);

        var result = controller.GetById(7);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ReportModel>(ok.Value);
        Assert.Equal(7, payload.Id);
        Assert.Equal("Ok", payload.Title);
        mockReportRepository.Verify(r => r.GetById(7), Times.Once);
    }

    [Fact]
    public void Create_Valid_ResponseContainsReportAndOccurrenceObjects()
    {
        var input = ReportFixture.CreateOrUpdate(title: "Buraco", description: "desc", userId: 1, location: Pt());
        var createdReport = ReportFixture.CreateOrUpdate(id: 201, title: input.Title, description: input.Description, userId: input.UserId, location: input.Location);
        var createdOccurrence = new Occurrence { Id = 301, ReportId = 201, ReportCount = 1, Status = OccurrenceStatus.WAITING, Type = input.Type, Priority = input.Priority, Location = Pt() };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Returns((createdReport, createdOccurrence));

        var result = controller.Create(input);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);
        Assert.True(created.RouteValues?.ContainsKey("id"));
        Assert.Equal(201, created.RouteValues!["id"]);

        var value = created.Value!;
        var t = value.GetType();
        var repObj = Assert.IsType<ReportModel>(t.GetProperty("report")!.GetValue(value)!);
        var occObj = Assert.IsType<Occurrence>(t.GetProperty("occurrence")!.GetValue(value)!);

        Assert.Equal(201, repObj.Id);
        Assert.Equal(301, occObj.Id);

        mockReportService.Verify(s => s.Create(It.Is<ReportModel>(r => r.Title == input.Title)), Times.Once);
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Create_ServiceThrowsArgumentException_ReturnsBadRequest_WithMessage()
    {
        var input = ReportFixture.CreateOrUpdate(title: "X", description: "Y", userId: 1, location: Pt());
        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Throws(new ArgumentException("invalid"));

        var result = controller.Create(input);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("invalid", bad.Value);
    }

    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError_WithMessage()
    {
        var input = ReportFixture.CreateOrUpdate(title: "X", description: "Y", userId: 1, location: Pt());
        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Throws(new Exception("fail"));

        var result = controller.Create(input);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
        Assert.Equal("fail", status.Value);
    }
}
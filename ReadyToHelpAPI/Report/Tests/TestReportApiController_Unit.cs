namespace readytohelpapi.Report.Tests;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using readytohelpapi.Common.Data;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Controllers;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Report.Models;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.ResponsibleEntity.Models;
using Xunit;

/// <summary>
/// This class contains all tests related to ReportApiController.
/// </summary>
[Trait("Category", "Unit")]
public class TestReportApiController_Unit
{
    private readonly Mock<IReportService> mockReportService;
    private readonly Mock<IReportRepository> mockReportRepository;
    private readonly Mock<AppDbContext> mockContext;
    private readonly ReportApiController controller;

    public TestReportApiController_Unit()
    {
        mockReportService = new Mock<IReportService>();
        mockReportRepository = new Mock<IReportRepository>();
        mockContext = new Mock<AppDbContext>();

        controller = new ReportApiController(
            mockReportService.Object,
            mockReportRepository.Object,
            mockContext.Object
        );
    }

    /// <summary>
    /// Creates a GeoPoint for testing.
    /// </summary>
    /// <returns>A GeoPoint instance.</returns>
    private static GeoPoint Pt() => new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 };

    /// <summary>
    /// Tests the Create method with a null report.
    /// </summary>
    [Fact]
    public void Create_NullReport_ReturnsBadRequest()
    {
        var result = controller.Create(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mockReportService.Verify(s => s.Create(It.IsAny<Report>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with a valid report.
    /// </summary>
    [Fact]
    public void Create_Valid_ReturnsCreatedAtAction_WithResponseObject()
    {
        var dto = new CreateReportDto
        {
            Title = "Buraco",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012,
        };

        var createdReport = ReportFixture.CreateOrUpdate(
            id: 101,
            title: dto.Title,
            description: dto.Description,
            userId: dto.UserId,
            location: Pt()
        );
        var createdOccurrence = new Occurrence
        {
            Id = 202,
            ReportId = 101,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 0,
            Location = Pt(),
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Returns((createdReport, createdOccurrence));

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);

        mockReportService.Verify(
            s =>
                s.Create(
                    It.Is<Report>(r =>
                        r.Title == dto.Title
                        && r.Description == dto.Description
                        && r.UserId == dto.UserId
                        && r.Location != null
                    )
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Tests the Create method with a service throwing ArgumentException.
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new CreateReportDto
        {
            Title = "X",
            Description = "Y",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012,
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Throws(new ArgumentException("invalid"));

        var result = controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// Tests the Create method with a service throwing a generic exception.
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var dto = new CreateReportDto
        {
            Title = "X",
            Description = "Y",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012,
        };

        mockReportService.Setup(s => s.Create(It.IsAny<Report>())).Throws(new Exception("fail"));

        var result = controller.Create(dto);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    /// Tests the GetById method with an invalid id.
    /// </summary>
    [Fact]
    public void GetById_InvalidId_ReturnsBadRequest()
    {
        var result = controller.GetById(0);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid id.", bad.Value);
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests the GetById method with a negative id.
    /// </summary>
    [Fact]
    public void GetById_NegativeId_ReturnsBadRequest()
    {
        var result = controller.GetById(-5);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid id.", bad.Value);
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with a valid report.
    /// </summary>
    [Fact]
    public void Create_Valid_ResponseContainsReportAndOccurrenceIds()
    {
        var dto = new CreateReportDto
        {
            Title = "Buraco",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012,
        };

        var createdReport = ReportFixture.CreateOrUpdate(
            id: 201,
            title: dto.Title,
            description: dto.Description,
            userId: dto.UserId,
            location: Pt()
        );
        var createdOccurrence = new Occurrence
        {
            Id = 301,
            ReportId = 201,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 0,
            Location = Pt(),
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Returns((createdReport, createdOccurrence));

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);
        Assert.True(created.RouteValues?.ContainsKey("id"));
        Assert.Equal(201, created.RouteValues!["id"]);

        var response = Assert.IsType<ReportResponseDto>(created.Value);
        Assert.Equal(201, response.ReportId);
        Assert.Equal(301, response.OccurrenceId);
        Assert.Equal(OccurrenceStatus.WAITING, response.OccurrenceStatus);
        Assert.Null(response.ResponsibleEntity);

        mockReportService.Verify(
            s => s.Create(It.Is<Report>(r => r.Title == dto.Title)),
            Times.Once
        );
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with an empty title.
    /// </summary>
    [Fact]
    public void Create_EmptyTitle_ThrowsArgumentException()
    {
        var dto = new CreateReportDto
        {
            Title = "",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 40.0,
            Longitude = -8.0,
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Throws(new ArgumentException("Title is required."));

        var result = controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// Tests the Create method with an empty description.
    /// </summary>
    [Fact]
    public void Create_EmptyDescription_ThrowsArgumentException()
    {
        var dto = new CreateReportDto
        {
            Title = "Title",
            Description = "",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 40.0,
            Longitude = -8.0,
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Throws(new ArgumentException("Description is required."));

        var result = controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// Tests the Create method with an invalid user id.
    /// </summary>
    [Fact]
    public void Create_InvalidUserId_ThrowsArgumentException()
    {
        var dto = new CreateReportDto
        {
            Title = "Title",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            UserId = 0,
            Latitude = 40.0,
            Longitude = -8.0,
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Throws(new ArgumentException("UserId must be greater than zero."));

        var result = controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

        [Fact]
    public void Create_WithResponsibleEntity_MapsContactInfo()
    {
        // usar AppDbContext real em memória para poder consultar ResponsibleEntities
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"reports-{Guid.NewGuid():N}")
            .Options;
        using var realCtx = new AppDbContext(opts);

        realCtx.ResponsibleEntities.Add(new ResponsibleEntity
        {
            Id = 55,
            Name = "Entidade XPTO",
            Email = "xpto@example.com",
            Address = "Rua A, 123",
            ContactPhone = 999111222
        });
        realCtx.SaveChanges();

        var localService = new Mock<IReportService>();
        var localRepo = new Mock<IReportRepository>();
        var localController = new ReportApiController(localService.Object, localRepo.Object, realCtx);

        var dto = new CreateReportDto
        {
            Title = "T",
            Description = "D",
            Type = OccurrenceType.FLOOD,
            UserId = 2,
            Latitude = 41.1,
            Longitude = -8.6
        };

        var createdReport = ReportFixture.CreateOrUpdate(id: 777, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 888,
            ReportId = 777,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 55,
            Location = Pt()
        };

        localService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Returns((createdReport, createdOccurrence));

        var result = localController.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ReportResponseDto>(created.Value);
        Assert.NotNull(response.ResponsibleEntity);
        Assert.Equal("Entidade XPTO", response.ResponsibleEntity!.Name);
        Assert.Equal("xpto@example.com", response.ResponsibleEntity.Email);
        Assert.Equal("Rua A, 123", response.ResponsibleEntity.Address);
        Assert.Equal(999111222, response.ResponsibleEntity.ContactPhone);
    }

       [Fact]
    public void Create_WithResponsibleEntity_NotFound_LeavesNull()
    {
        var dto = new CreateReportDto
        {
            Title = "T",
            Description = "D",
            Type = OccurrenceType.FLOOD,
            UserId = 2,
            Latitude = 41.1,
            Longitude = -8.6
        };

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"reports-{Guid.NewGuid():N}")
            .Options;
        using var realCtx = new AppDbContext(opts);

        var createdReport = ReportFixture.CreateOrUpdate(id: 10, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 20,
            ReportId = 10,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 999, // não existe
            Location = Pt()
        };

        var localService = new Mock<IReportService>();
        var localRepo = new Mock<IReportRepository>();
        localService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Returns((createdReport, createdOccurrence));

        var localController = new ReportApiController(localService.Object, localRepo.Object, realCtx);

        var result = localController.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ReportResponseDto>(created.Value);
        Assert.Null(response.ResponsibleEntity);
    }

    [Fact]
    public void Create_PassesLatLon_ToService()
    {
        var dto = new CreateReportDto
        {
            Title = "LL",
            Description = "geo",
            Type = OccurrenceType.ROAD_DAMAGE,
            UserId = 3,
            Latitude = 40.1234,
            Longitude = -7.5678
        };

        Report? captured = null;
        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Callback<Report>(r => captured = r)
            .Returns((ReportFixture.CreateOrUpdate(id: 1, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt()),
                      new Occurrence { Id = 2, ReportId = 1, Status = OccurrenceStatus.WAITING, Type = dto.Type, Location = Pt() }));

        var result = controller.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(captured);
        Assert.NotNull(captured!.Location);
        Assert.Equal(dto.Latitude, captured.Location!.Latitude);
        Assert.Equal(dto.Longitude, captured.Location.Longitude);
    }

    [Fact]
    public void GetById_Found_ReturnsOk_WithReport()
    {
        var report = ReportFixture.CreateOrUpdate(id: 42, title: "R", description: "D", userId: 1, location: Pt());
        mockReportRepository.Setup(r => r.GetById(42)).Returns(report);

        var result = controller.GetById(42);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(report, ok.Value);
        mockReportRepository.Verify(r => r.GetById(42), Times.Once);
    }

    [Fact]
    public void GetById_NotFound_ReturnsNotFound()
    {
        mockReportRepository.Setup(r => r.GetById(It.IsAny<int>())).Returns((Report?)null);

        var result = controller.GetById(1234);

        Assert.IsType<NotFoundResult>(result);
        mockReportRepository.Verify(r => r.GetById(1234), Times.Once);
    }

    [Fact]
    public void Create_ServiceValidationError_ReturnsBadRequest_WithShape()
    {
        var dto = new CreateReportDto
        {
            Title = "X",
            Description = "Y",
            Type = OccurrenceType.FOREST_FIRE,
            UserId = 9,
            Latitude = 41,
            Longitude = -8
        };

        mockReportService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Throws(new ArgumentException("Title is required."));

        var result = controller.Create(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);

        var json = JsonSerializer.Serialize(bad.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        Assert.NotNull(dict);
        Assert.Equal("validation_error", dict!["error"].ToString());
        Assert.Equal("Title is required.", dict["detail"].ToString());
    }
}

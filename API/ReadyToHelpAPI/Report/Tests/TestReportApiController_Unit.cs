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
    private readonly Mock<AppDbContext> mockContext;
    private readonly ReportApiController controller;

    public TestReportApiController_Unit()
    {
        mockReportService = new Mock<IReportService>();
        mockContext = new Mock<AppDbContext>();

        controller = new ReportApiController(
            mockReportService.Object,
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
        var localController = new ReportApiController(localService.Object, realCtx);

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

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
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
            ResponsibleEntityId = 999,
            Location = Pt()
        };

        var localService = new Mock<IReportService>();
        localService
            .Setup(s => s.Create(It.IsAny<Report>()))
            .Returns((createdReport, createdOccurrence));

        var localController = new ReportApiController(localService.Object, realCtx);

        var result = localController.Create(dto);

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
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

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.NotNull(captured);
        Assert.NotNull(captured!.Location);
        Assert.Equal(dto.Latitude, captured.Location!.Latitude);
        Assert.Equal(dto.Longitude, captured.Location.Longitude);
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

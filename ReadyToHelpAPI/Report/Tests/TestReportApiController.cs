namespace readytohelpapi.Report.Tests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ReportModel = readytohelpapi.Report.Models.Report;
using readytohelpapi.Report.Controllers;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Common.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using readytohelpapi.Occurrence.Services;

/// <summary>
/// This class contains all tests related to ReportApiController.
/// </summary>
[Trait("Category", "Integration")]
public class TestReportApiController : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly Mock<IReportService> mockReportService;
    private readonly Mock<IReportRepository> mockReportRepository;
    private readonly AppDbContext context;
    private readonly ReportApiController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestReportApiController"/> class.
    /// </summary>
    /// <param name="fixture">The database fixture.</param>
    public TestReportApiController(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        this.context = this.fixture.Context;

        mockReportService = new Mock<IReportService>();
        mockReportRepository = new Mock<IReportRepository>();
        controller = new ReportApiController(mockReportService.Object, mockReportRepository.Object, context);
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
        mockReportService.Verify(s => s.Create(It.IsAny<ReportModel>()), Times.Never);
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
            Longitude = -8.2012
        };

        var createdReport = ReportFixture.CreateOrUpdate(id: 101, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 202,
            ReportId = 101,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 0,
            Location = Pt()
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
                         .Returns((createdReport, createdOccurrence));

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);

        mockReportService.Verify(s => s.Create(It.Is<ReportModel>(r =>
            r.Title == dto.Title &&
            r.Description == dto.Description &&
            r.UserId == dto.UserId &&
            r.Location != null
        )), Times.Once);
    }

    /// <summary>
    /// Tests the Create method with a responsible entity.
    /// </summary>
    [Fact]
    public void Create_WithResponsibleEntity_ReturnsEntityInResponse()
    {
        var dto = new CreateReportDto
        {
            Title = "Incêndio",
            Description = "Fogo na mata",
            Type = OccurrenceType.FOREST_FIRE,
            UserId = 2,
            Latitude = 38.720,
            Longitude = -9.149
        };

        var geometryFactory = new NetTopologySuite.Geometries.GeometryFactory(
             new NetTopologySuite.Geometries.PrecisionModel(),
             4326
         );

        var coordinates = new[]
        {
            new NetTopologySuite.Geometries.Coordinate(-9.158, 38.715),
            new NetTopologySuite.Geometries.Coordinate(-9.158, 38.725),
            new NetTopologySuite.Geometries.Coordinate(-9.14, 38.725),
            new NetTopologySuite.Geometries.Coordinate(-9.14, 38.715),
            new NetTopologySuite.Geometries.Coordinate(-9.158, 38.715)
        };

        var poly = geometryFactory.CreatePolygon(coordinates);
        var multi = geometryFactory.CreateMultiPolygon(new[] { poly });

        var entity = new ResponsibleEntity.Models.ResponsibleEntity
        {
            Id = 10,
            Name = "Bombeiros Lisboa",
            Email = "bombeiros@lisboa.pt",
            Address = "Rua X, Lisboa",
            ContactPhone = 213456789,
            Type = ResponsibleEntity.Models.ResponsibleEntityType.BOMBEIROS,
            GeoArea = multi
        };

        context.ResponsibleEntities.Add(entity);
        context.SaveChanges();

        var createdReport = ReportFixture.CreateOrUpdate(id: 150, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 250,
            ReportId = 150,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 10,
            Location = Pt()
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Returns((createdReport, createdOccurrence));

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ReportResponseDto>(created.Value);

        Assert.NotNull(response.ResponsibleEntity);
        Assert.Equal("Bombeiros Lisboa", response.ResponsibleEntity.Name);
        Assert.Equal("bombeiros@lisboa.pt", response.ResponsibleEntity.Email);
        Assert.Equal("Rua X, Lisboa", response.ResponsibleEntity.Address);
        Assert.Equal(213456789, response.ResponsibleEntity.ContactPhone);
    }

    /// <summary>
    /// Tests the Create method when the responsible entity is not found.
    /// </summary>
    [Fact]
    public void Create_ResponsibleEntityNotFound_ReturnsNullEntity()
    {
        var dto = new CreateReportDto
        {
            Title = "Teste",
            Description = "desc",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 40.0,
            Longitude = -8.0
        };

        var createdReport = ReportFixture.CreateOrUpdate(id: 160, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 260,
            ReportId = 160,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 999,
            Location = Pt()
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Returns((createdReport, createdOccurrence));

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ReportResponseDto>(created.Value);

        Assert.Null(response.ResponsibleEntity);
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
            Longitude = -8.2012
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
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
            Longitude = -8.2012
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
                         .Throws(new Exception("fail"));

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
    /// Tests the GetById method when the report is not found.
    /// </summary>
    [Fact]
    public void GetById_NotFound_ReturnsNotFound()
    {
        mockReportRepository.Setup(r => r.GetById(42)).Returns((ReportModel?)null);

        var result = controller.GetById(42);

        Assert.IsType<NotFoundResult>(result);
        mockReportRepository.Verify(r => r.GetById(42), Times.Once);
    }

    /// <summary>
    /// Tests the GetById method when the report is found.
    /// </summary>
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
            Longitude = -8.2012
        };

        var createdReport = ReportFixture.CreateOrUpdate(id: 201, title: dto.Title, description: dto.Description, userId: dto.UserId, location: Pt());
        var createdOccurrence = new Occurrence
        {
            Id = 301,
            ReportId = 201,
            ReportCount = 1,
            Status = OccurrenceStatus.WAITING,
            Type = dto.Type,
            ResponsibleEntityId = 0,
            Location = Pt()
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Returns((createdReport, createdOccurrence));

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

        mockReportService.Verify(s => s.Create(It.Is<ReportModel>(r => r.Title == dto.Title)), Times.Once);
        mockReportRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with a responsible entity.
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsArgumentException_ReturnsBadRequest_WithMessage()
    {
        var dto = new CreateReportDto
        {
            Title = "X",
            Description = "Y",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Throws(new ArgumentException("invalid"));

        var result = controller.Create(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var errorObj = bad.Value!;
        var errorProp = errorObj.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("validation_error", errorProp.GetValue(errorObj));
    }

    /// <summary>
    /// Tests the Create method with a service throwing a generic exception.
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError_WithMessage()
    {
        var dto = new CreateReportDto
        {
            Title = "X",
            Description = "Y",
            Type = OccurrenceType.FLOOD,
            UserId = 1,
            Latitude = 41.3678,
            Longitude = -8.2012
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>())).Throws(new Exception("fail"));

        var result = controller.Create(dto);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
        var errorObj = status.Value!;
        var errorProp = errorObj.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("internal_server_error", errorProp.GetValue(errorObj));
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
            Longitude = -8.0
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
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
            Longitude = -8.0
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
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
            Longitude = -8.0
        };

        mockReportService.Setup(s => s.Create(It.IsAny<ReportModel>()))
            .Throws(new ArgumentException("UserId must be greater than zero."));

        var result = controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Tests.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq; // ADDED

namespace readytohelpapi.Occurrence.Tests;

/// <summary>
///   This class contains all unit tests for OccurrenceApiController,
///   following the same approach and documentation used in TestUserApiController.
/// </summary>
public class TestOccurrenceApiController : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly Mock<IOccurrenceService> mockOccurrenceService;
    private readonly OccurrenceApiController controller;

    /// <summary>
    ///   Initializes a new instance of TestOccurrenceApiController.
    /// </summary>
    public TestOccurrenceApiController(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();

        mockOccurrenceService = new Mock<IOccurrenceService>();
        controller = new OccurrenceApiController(mockOccurrenceService.Object);
    }

    /// <summary>
    ///   Tests Create with a null occurrence.
    /// </summary>
    [Fact]
    public void Create_NullOccurrence_ReturnsBadRequest()
    {
        var result = controller.Create(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Create with a valid occurrence.
    /// </summary>
    [Fact]
    public void Create_ValidOccurrence_ReturnsCreatedAtAction()
    {
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(id: 0, title: "T");
        var created = OccurrenceFixture.CreateOrUpdateOccurrence(id: 10, title: "T");
        mockOccurrenceService.Setup(s => s.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var result = controller.Create(input);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), createdAt.ActionName);
    }

    /// <summary>
    ///   Tests Create when service throws ArgumentException.
    /// </summary>
    [Fact]
    public void CreateOccurrence_ServiceThrowsArgument_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.Create(It.IsAny<Models.Occurrence>()))
                   .Throws(new ArgumentException("invalid"));
        var result = controller.Create(new Models.Occurrence { Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 1 });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Create when service throws a generic exception.
    ///   Expects Internal Server Error (500).
    /// </summary>
    [Fact]
    public void CreateOccurrence_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.Create(It.IsAny<Models.Occurrence>()))
                   .Throws(new Exception("unexpected"));
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(id: 0, title: "X");
        var result = controller.Create(input);
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests Create with a valid occurrence without ReportId (null).
    /// </summary>
    [Fact]
    public void Create_ValidOccurrenceWithoutReportId_ReturnsCreatedAtAction()
    {
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(id: 0, title: "T");
        input.ReportId = null;

        var created = OccurrenceFixture.CreateOrUpdateOccurrence(id: 10, title: "T");
        created.ReportId = null;

        mockOccurrenceService.Setup(s => s.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var result = controller.Create(input);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), createdAt.ActionName);
    }

    /// <summary>
    ///   Tests Update with a null occurrence.
    /// </summary>
    [Fact]
    public void Update_NullOccurrence_ReturnsBadRequest()
    {
        var result = controller.Update(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Update with a valid occurrence.
    /// </summary>
    [Fact]
    public void Update_ValidOccurrence_ReturnsOk()
    {
        var toUpdate = OccurrenceFixture.CreateOrUpdateOccurrence(id: 5, title: "TT");
        mockOccurrenceService.Setup(s => s.Update(It.IsAny<Models.Occurrence>())).Returns(toUpdate);

        var result = controller.Update(toUpdate);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(toUpdate, ok.Value);
    }

    /// <summary>
    ///   Tests Update when occurrence is not found.
    /// </summary>
    [Fact]
    public void Update_NotFoundOccurrence_ReturnsNotFoundObject()
    {
        mockOccurrenceService.Setup(s => s.Update(It.IsAny<Models.Occurrence>()))
                   .Throws(new KeyNotFoundException("not found"));

        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(id: 999));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    ///   Tests Update when service throws ArgumentException.
    /// </summary>
    [Fact]
    public void UpdateOccurrence_ServiceThrowsArgument_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.Update(It.IsAny<Models.Occurrence>()))
                   .Throws(new ArgumentException("bad"));
        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(id: 1));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Update when service throws a generic exception.
    ///   Expects Internal Server Error (500).
    /// </summary>
    [Fact]
    public void UpdateOccurrence_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.Update(It.IsAny<Models.Occurrence>()))
                   .Throws(new Exception("db failed"));
        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(id: 2));
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests Delete with an invalid id.
    /// </summary>
    [Fact]
    public void DeleteOccurrence_InvalidId_ReturnsBadRequest()
    {
        var result = controller.Delete(0);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Delete when the occurrence does not exist.
    /// </summary>
    [Fact]
    public void DeleteOccurrence_NotFound_ReturnsNotFoundObject()
    {
        mockOccurrenceService.Setup(s => s.Delete(123)).Throws(new KeyNotFoundException("nope"));
        var result = controller.Delete(123);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    ///   Tests Delete when service throws ArgumentException.
    /// </summary>
    [Fact]
    public void DeleteOccurrence_ServiceThrowsArgument_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.Delete(It.IsAny<int>()))
                   .Throws(new ArgumentException("invalid id"));
        var result = controller.Delete(10);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests Delete when the occurrence exists.
    /// </summary>
    [Fact]
    public void DeleteOccurrence_Existing_ReturnsOk()
    {
        var del = OccurrenceFixture.CreateOrUpdateOccurrence(id: 3);
        mockOccurrenceService.Setup(s => s.Delete(3)).Returns(del);

        var result = controller.Delete(3);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(del, ok.Value);
    }

    /// <summary>
    ///   Tests Delete when service throws a generic exception.
    ///   Expects Internal Server Error (500).
    /// </summary>
    [Fact]
    public void DeleteOccurrence_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.Delete(It.IsAny<int>()))
                   .Throws(new Exception("boom"));
        var result = controller.Delete(5);
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetById with an invalid id.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_Invalid_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(It.IsAny<int>()))
                   .Throws(new ArgumentException("invalid"));
        var result = controller.GetById(0);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetById when the occurrence is not found.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_NotFound_ReturnsNotFound()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(It.IsAny<int>()))
                   .Throws(new KeyNotFoundException("not found"));
        var result = controller.GetById(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetById with an existing occurrence.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_Valid_ReturnsOkWithOccurrence()
    {
        var occ = OccurrenceFixture.CreateOrUpdateOccurrence(id: 42, title: "Found");
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(42)).Returns(occ);

        var result = controller.GetById(42);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<OccurrenceDetailsDto>(ok.Value);
        Assert.Equal(42, dto.Id);
    }

    /// <summary>
    ///   Tests GetById when service throws a generic exception.
    ///   Expects Internal Server Error (500).
    /// </summary>
    [Fact]
    public void GetOccurrenceById_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(It.IsAny<int>()))
                   .Throws(new Exception("db"));
        var result = controller.GetById(50);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetById with an existing occurrence.
    /// </summary>
    [Fact]
    public void GetOccurrenceById_Valid_ReturnsOkWithDetailsDto()
    {
        var occ = OccurrenceFixture.CreateOrUpdateOccurrence(
            id: 42,
            title: "Found",
            description: "Desc",
            type: OccurrenceType.FOREST_FIRE,
            status: OccurrenceStatus.ACTIVE,
            priority: PriorityLevel.HIGH,
            latitude: 1.23,
            longitude: 4.56,
            reportCount: 3,
            responsibleEntityId: 0
        );
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(42)).Returns(occ);

        var result = controller.GetById(42);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<OccurrenceDetailsDto>(ok.Value);
        Assert.Equal(42, dto.Id);
        Assert.Equal("Found", dto.Title);
        Assert.Equal("Desc", dto.Description);
        Assert.Equal(OccurrenceType.FOREST_FIRE, dto.Type);
        Assert.Equal(OccurrenceStatus.ACTIVE, dto.Status);
        Assert.Equal(PriorityLevel.HIGH, dto.Priority);
        Assert.Equal(1.23, dto.Latitude);
        Assert.Equal(4.56, dto.Longitude);
        Assert.Equal(occ.CreationDateTime, dto.CreationDateTime);
        Assert.Equal(occ.EndDateTime, dto.EndDateTime);
        Assert.Null(dto.ResponsibleEntityId);
        Assert.Equal(3, dto.ReportCount);
    }

    /// <summary>
    ///   Tests GetById when the service returns null (not found).
    /// </summary>
    [Fact]
    public void GetOccurrenceById_ServiceReturnsNull_ReturnsNotFound()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(777)).Returns((Models.Occurrence?)null);

        var result = controller.GetById(777);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// <summary>
    ///   Ensures GetById has [Authorize] and both route templates (singular and plural).
    /// </summary>
    [Fact]
    public void GetById_HasAuthorizeAndRoutes()
    {
        var mi = typeof(OccurrenceApiController).GetMethod("GetById");
        Assert.NotNull(mi);

        var authorizeAttrs = mi!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        Assert.NotEmpty(authorizeAttrs);

        var httpGets = mi.GetCustomAttributes(typeof(HttpGetAttribute), true).Cast<HttpGetAttribute>().ToArray();
        Assert.True(httpGets.Length >= 1);

        var templates = httpGets.Select(a => a.Template ?? string.Empty).ToArray();
        Assert.Contains(templates, t => t.Equals("{id:int}", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, t => t.Contains("api/occurrences", StringComparison.OrdinalIgnoreCase));
    }
}
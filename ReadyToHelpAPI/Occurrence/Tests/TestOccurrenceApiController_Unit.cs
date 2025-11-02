namespace readytohelpapi.Occurrence.Tests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Tests.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.Occurrence.DTOs;

/// <summary>
///   This class contains all unit tests for OccurrenceApiController,
///   following the same approach and documentation used in TestUserApiController.
/// </summary>
[Trait("Category", "Unit")]
public class TestOccurrenceApiController
{
    private readonly Mock<IOccurrenceService> mockOccurrenceService;
    private readonly OccurrenceApiController controller;

    /// <summary>
    ///   Initializes a new instance of TestOccurrenceApiController.
    /// </summary>
    public TestOccurrenceApiController()
    {
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
        var options = new OccurrenceFixtureDto { Id = 0, Title = "T" };
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);

        var createdOptions = new OccurrenceFixtureDto { Id = 10, Title = "T" };
        var created = OccurrenceFixture.CreateOrUpdateOccurrence(options: createdOptions);
        mockOccurrenceService.Setup(s => s.CreateAdminOccurrence(It.IsAny<Models.Occurrence>())).Returns(created);

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
        mockOccurrenceService.Setup(s => s.CreateAdminOccurrence(It.IsAny<Models.Occurrence>()))
                   .Throws(new ArgumentException("invalid"));
        var options = new OccurrenceFixtureDto { Id = 0, Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 1 };
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
        var result = controller.Create(input);
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
        var options = new OccurrenceFixtureDto { Id = 0, Title = "X", Description = "D", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.MEDIUM, ProximityRadius = 5 };
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
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
        var options = new OccurrenceFixtureDto { Id = 0, Title = "T" };
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
        input.ReportId = null;

        var createdOptions = new OccurrenceFixtureDto { Id = 10, Title = "T" };
        var created = OccurrenceFixture.CreateOrUpdateOccurrence(options: createdOptions);
        created.ReportId = null;

        mockOccurrenceService.Setup(s => s.CreateAdminOccurrence(It.IsAny<Models.Occurrence>())).Returns(created);

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
        var options = new OccurrenceFixtureDto { Id = 5, Title = "TT" };
        var toUpdate = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
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

        var options = new OccurrenceFixtureDto { Id = 999 };
        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(options: options));
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

        var options = new OccurrenceFixtureDto { Id = 1 };
        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(options: options));
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

        var options = new OccurrenceFixtureDto { Id = 2 };
        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(options: options));
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
        var delOptions = new OccurrenceFixtureDto { Id = 3 };
        var del = OccurrenceFixture.CreateOrUpdateOccurrence(options: delOptions);
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
        var options = new OccurrenceFixtureDto { Id = 42, Title = "Found" };
        var occ = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
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
        var options = new OccurrenceFixtureDto
        {
            Id = 42,
            Title = "Found",
            Description = "Desc",
            Type = OccurrenceType.FOREST_FIRE,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.HIGH,
            Latitude = 1.23,
            Longitude = 4.56,
            ReportCount = 3,
            ResponsibleEntityId = 0
        };
        var occ = OccurrenceFixture.CreateOrUpdateOccurrence(options: options);
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
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(777)).Returns((Occurrence)null!);

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

    /// <summary>
    ///   Tests GetAll returning a list of occurrences.
    /// </summary>
    [Fact]
    public void GetAll_ReturnsOkWithList()
    {
        var occ1Options = new OccurrenceFixtureDto
        {
            Id = 42,
            Title = "Found",
            Description = "Desc",
            Type = OccurrenceType.FOREST_FIRE,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.HIGH,
            Latitude = 1.23,
            Longitude = 4.56,
            ReportCount = 3,
            ResponsibleEntityId = 0
        };
        var occ2Options = new OccurrenceFixtureDto
        {
            Id = 43,
            Title = "Lost",
            Description = "Desc2",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.RESOLVED,
            Priority = PriorityLevel.LOW,
            Latitude = 7.89,
            Longitude = 0.12,
            ReportCount = 1,
            ResponsibleEntityId = 5
        };

        var occurrences = new List<Occurrence>
    {
        OccurrenceFixture.CreateOrUpdateOccurrence(options: occ1Options),
        OccurrenceFixture.CreateOrUpdateOccurrence(options: occ2Options)
    };

        mockOccurrenceService
            .Setup(s => s.GetAllOccurrences(1, 10, "Title", "asc", ""))
            .Returns(occurrences);

        var result = controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(occurrences, ok.Value);
    }

    /// <summary>
    ///   Tests GetAll when service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockOccurrenceService
            .Setup(s => s.GetAllOccurrences(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException("bad"));

        var result = controller.GetAll();

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetAll when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockOccurrenceService
            .Setup(s => s.GetAllOccurrences(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("db fail"));

        var result = controller.GetAll();

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetAllActive returning map DTO results.
    /// </summary>
    [Fact]
    public void GetAllActive_ReturnsOkWithMappedList()
    {
        var options1 = new OccurrenceFixtureDto
        {
            Id = 42,
            Title = "Found",
            Description = "Desc",
            Type = OccurrenceType.FOREST_FIRE,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.HIGH,
            Latitude = 1.23,
            Longitude = 4.56,
            ReportCount = 3,
            ResponsibleEntityId = 0
        };
        var options2 = new OccurrenceFixtureDto
        {
            Id = 43,
            Title = "Lost",
            Description = "Desc2",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.RESOLVED,
            Priority = PriorityLevel.LOW,
            Latitude = 1.23,
            Longitude = 4.56,
            ReportCount = 1,
            ResponsibleEntityId = 5
        };

        var active = new List<readytohelpapi.Occurrence.Models.Occurrence>
        {
            OccurrenceFixture.CreateOrUpdateOccurrence(options: options1),
            OccurrenceFixture.CreateOrUpdateOccurrence(options: options2)
        };

        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(1, 50, null, null, null))
            .Returns(active);

        var result = controller.GetAllActive();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<OccurrenceMapDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    /// <summary>
    ///   Tests GetAllActive returning NotFound.
    /// </summary>
    [Fact]
    public void GetAllActive_NoOccurrences_ReturnsNotFound()
    {
        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(1, 50, null, null, null))
            .Returns(new List<readytohelpapi.Occurrence.Models.Occurrence>());

        var result = controller.GetAllActive();

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetAllActive when service throws.
    /// </summary>
    [Fact]
    public void GetAllActive_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(It.IsAny<int>(), It.IsAny<int>(), null, null, null))
            .Throws(new Exception("db"));

        var result = controller.GetAllActive();

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }
}
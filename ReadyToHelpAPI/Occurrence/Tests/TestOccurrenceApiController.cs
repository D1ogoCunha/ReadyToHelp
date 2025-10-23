using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Tests.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;

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
        Assert.IsType<BadRequestObjectResult>(result);
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
        Assert.IsType<NotFoundResult>(result);
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
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(occ, ok.Value);
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
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetAll returning a list with default parameters.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 1), OccurrenceFixture.CreateOrUpdateOccurrence(id: 2) };
        mockOccurrenceService.Setup(s => s.GetAllOccurrences(1, 10, "Title", "asc", "")).Returns(list);

        var resp = controller.GetAll();
        var action = Assert.IsType<ActionResult<List<Models.Occurrence>>>(resp);
        var ok = Assert.IsType<OkObjectResult>(action.Result!);
        Assert.Equal(list, ok.Value);
    }

    /// <summary>
    ///   Tests GetAll with custom parameters.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_CustomParameters_ReturnsOk()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 7) };
        mockOccurrenceService.Setup(s => s.GetAllOccurrences(2, 5, "Type", "desc", "fire")).Returns(list);

        var resp = controller.GetAll(2, 5, "Type", "desc", "fire");
        var action = Assert.IsType<ActionResult<List<Models.Occurrence>>>(resp);
        var ok = Assert.IsType<OkObjectResult>(action.Result!);
        Assert.Equal(list, ok.Value);
    }

    /// <summary>
    ///   Tests GetAll when service throws ArgumentException.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_ServiceThrowsArgument_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.GetAllOccurrences(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Throws(new ArgumentException("bad params"));

        var resp = controller.GetAll();
        var action = Assert.IsType<ActionResult<List<Models.Occurrence>>>(resp);
        Assert.IsType<BadRequestObjectResult>(action.Result!);
    }

    /// <summary>
    ///   Tests GetAll when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetAllOccurrences_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetAllOccurrences(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Throws(new Exception("db"));

        var resp = controller.GetAll();
        var action = Assert.IsType<ActionResult<List<Models.Occurrence>>>(resp);
        var status = Assert.IsType<ObjectResult>(action.Result!);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetByTitle with an invalid title.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_Invalid_ReturnsBadRequest()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceByTitle(It.IsAny<string>())).Throws(new ArgumentException("bad"));
        var result = controller.GetByTitle("");
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetByTitle with a valid result.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_Valid_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 11, title: "AAA") };
        mockOccurrenceService.Setup(s => s.GetOccurrenceByTitle("AAA")).Returns(list);

        var result = controller.GetByTitle("AAA");
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, ok.Value);
    }

    /// <summary>
    ///   Tests GetByTitle returning an empty list.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_EmptyList_ReturnsOkWithEmptyList()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceByTitle("none")).Returns(new List<Models.Occurrence>());
        var result = controller.GetByTitle("none");
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<Models.Occurrence>>(ok.Value);
        Assert.Empty(value);
    }

    /// <summary>
    ///   Tests GetByTitle when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetOccurrenceByTitle_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrenceByTitle(It.IsAny<string>()))
                   .Throws(new Exception("db"));
        var result = controller.GetByTitle("X");
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetByType with a valid result.
    /// </summary>
    [Fact]
    public void GetOccurrenceByType_Valid_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 21, type: OccurrenceType.FLOOD) };
        mockOccurrenceService.Setup(s => s.GetOccurrencesByType(OccurrenceType.FLOOD)).Returns(list);

        var result = controller.GetByType(OccurrenceType.FLOOD);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, ok.Value);
    }

    /// <summary>
    ///   Tests GetByType when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetOccurrenceByType_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrencesByType(It.IsAny<OccurrenceType>()))
                   .Throws(new Exception("db"));
        var result = controller.GetByType(OccurrenceType.FOREST_FIRE);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetByPriority with a valid result.
    /// </summary>
    [Fact]
    public void GetOccurrenceByPriority_Valid_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 31, priority: PriorityLevel.HIGH) };
        mockOccurrenceService.Setup(s => s.GetOccurrencesByPriority(PriorityLevel.HIGH)).Returns(list);

        var result = controller.GetByPriority(PriorityLevel.HIGH);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, ok.Value);
    }

    /// <summary>
    ///   Tests GetByPriority when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetOccurrenceByPriority_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetOccurrencesByPriority(It.IsAny<PriorityLevel>()))
                   .Throws(new Exception("db"));
        var result = controller.GetByPriority(PriorityLevel.LOW);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests GetAllActive with a valid result.
    /// </summary>
    [Fact]
    public void GetAllActiveOccurrences_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 41, status: OccurrenceStatus.ACTIVE, latitude: 1, longitude: 2) };
        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(1, 50, null, null, null)).Returns(list);

        var result = controller.GetAllActive(1, 50, null, null, null);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<OccurrenceMapDto>>(ok.Value);
        Assert.Single(value);
        Assert.Equal(41, value[0].Id);
        Assert.Equal(1, value[0].Latitude);
        Assert.Equal(2, value[0].Longitude);
    }

    /// <summary>
    ///   Tests GetAllActive returning an empty list -> NotFound.
    /// </summary>
    [Fact]
    public void GetAllActiveOccurrences_Empty_ReturnsNotFound()
    {
        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(1, 50, null, null, null)).Returns(new List<Models.Occurrence>());
        var result = controller.GetAllActive(1, 50, null, null, null);
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    ///   Tests GetAllActive when service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetAllActiveOccurrences_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockOccurrenceService.Setup(s => s.GetAllActiveOccurrences(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OccurrenceType?>(), It.IsAny<PriorityLevel?>(), It.IsAny<int?>()
        )).Throws(new Exception("db"));
        var result = controller.GetAllActive();
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }
}
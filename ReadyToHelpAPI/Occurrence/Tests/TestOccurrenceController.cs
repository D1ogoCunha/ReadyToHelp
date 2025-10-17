using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Tests.Fixtures;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

public class TestOccurrenceApiController
{
    private readonly Mock<IOccurrenceService> mockService = new();
    private readonly OccurrenceApiController controller;

    public TestOccurrenceApiController()
    {
        controller = new OccurrenceApiController(mockService.Object);
    }

    [Fact]
    public void Create_Null_ReturnsBadRequest()
    {
        var result = controller.Create(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_Valid_ReturnsCreatedAtAction()
    {
        var input = OccurrenceFixture.CreateOrUpdateOccurrence(id: 0, title: "T");
        var created = OccurrenceFixture.CreateOrUpdateOccurrence(id: 10, title: "T");
        mockService.Setup(s => s.Create(It.IsAny<Models.Occurrence>())).Returns(created);

        var result = controller.Create(input);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), createdAt.ActionName);
    }

    [Fact]
    public void Create_ServiceThrowsArgument_ReturnsBadRequest()
    {
        mockService.Setup(s => s.Create(It.IsAny<Models.Occurrence>()))
                   .Throws(new ArgumentException("invalid"));
        var result = controller.Create(new Models.Occurrence { Title = "t", Description = "d", Type = OccurrenceType.FLOOD, Priority = PriorityLevel.LOW, ProximityRadius = 1 });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_Null_ReturnsBadRequest()
    {
        var result = controller.Update(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_Valid_ReturnsOk()
    {
        var toUpdate = OccurrenceFixture.CreateOrUpdateOccurrence(id: 5, title: "TT");
        mockService.Setup(s => s.Update(It.IsAny<Models.Occurrence>())).Returns(toUpdate);

        var result = controller.Update(toUpdate);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(toUpdate, ok.Value);
    }

    [Fact]
    public void Update_NotFound_ReturnsNotFound()
    {
        mockService.Setup(s => s.Update(It.IsAny<Models.Occurrence>()))
                   .Throws(new KeyNotFoundException("not found"));

        var result = controller.Update(OccurrenceFixture.CreateOrUpdateOccurrence(id: 999));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_InvalidId_ReturnsBadRequest()
    {
        var result = controller.Delete(0);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Delete_NotFound_ReturnsNotFound()
    {
        mockService.Setup(s => s.Delete(123)).Throws(new KeyNotFoundException("nope"));
        var result = controller.Delete(123);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_Existing_ReturnsOk()
    {
        var del = OccurrenceFixture.CreateOrUpdateOccurrence(id: 3);
        mockService.Setup(s => s.Delete(3)).Returns(del);

        var result = controller.Delete(3);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(del, ok.Value);
    }

    [Fact]
    public void GetById_Invalid_ReturnsBadRequest()
    {
        mockService.Setup(s => s.GetOccurrenceById(It.IsAny<int>()))
                   .Throws(new ArgumentException("invalid"));
        var result = controller.GetById(0);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetById_NotFound_ReturnsNotFound()
    {
        mockService.Setup(s => s.GetOccurrenceById(It.IsAny<int>()))
                   .Throws(new KeyNotFoundException("not found"));
        var result = controller.GetById(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetAll_ReturnsOkWithList()
    {
        var list = new List<Models.Occurrence> { OccurrenceFixture.CreateOrUpdateOccurrence(id: 1), OccurrenceFixture.CreateOrUpdateOccurrence(id: 2) };
        mockService.Setup(s => s.GetAllOccurrences(1, 10, "Title", "asc", "")).Returns(list);

        var resp = controller.GetAll();
        var action = Assert.IsType<ActionResult<List<Models.Occurrence>>>(resp);
        var ok = Assert.IsType<OkObjectResult>(action.Result!);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public void GetByTitle_Invalid_ReturnsBadRequest()
    {
        mockService.Setup(s => s.GetOccurrenceByTitle(It.IsAny<string>())).Throws(new ArgumentException("bad"));
        var result = controller.GetByTitle("");
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
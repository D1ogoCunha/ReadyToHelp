namespace readytohelpapi.Feedback.Tests;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Feedback.Controllers;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;
using readytohelpapi.Feedback.Tests.Fixtures;
using Xunit;



[Trait("Category", "Unit")]
public class TestFeedbackApiController_Unit
{
    private readonly Mock<IFeedbackService> mockService;
    private readonly FeedbackApiController controller;

    public TestFeedbackApiController_Unit()
    {
        mockService = new Mock<IFeedbackService>();
        controller = new FeedbackApiController(mockService.Object);
    }

    /// <summary>
    /// Tests the Create method with null feedback.
    /// </summary>
    [Fact]
    public void Create_NullFeedback_ReturnsBadRequest()
    {
        var result = controller.Create(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// Tests the Create method with valid feedback.
    /// </summary>
    [Fact]
    public void Create_ValidFeedback_ReturnsCreatedAtAction()
    {
        var fb = FeedbackFixture.Create(userId: 1, occurrenceId: 1);
        fb.Id = 10;
        mockService.Setup(s => s.Create(It.IsAny<Feedback>())).Returns(fb);

        var result = controller.Create(new Feedback { UserId = 1, OccurrenceId = 1 });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), created.ActionName);
        Assert.IsType<Feedback>(created.Value);
        Assert.Equal(10, ((Feedback)created.Value).Id);
    }

    /// <summary>
    /// Tests the Create method when the service throws an exception indicating the occurrence or user is missing.
    /// </summary>
    [Fact]
    public void Create_ReturnsNotFound_WhenServiceThrowsOccurrenceOrUserMissing()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<Feedback>()))
            .Throws(new ArgumentException("Occurrence does not exist"));

        var result = controller.Create(new Feedback { UserId = 1, OccurrenceId = 999 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Tests the Create method when the service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Create_ReturnsBadRequest_WhenServiceThrowsArgumentExceptionOther()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<Feedback>()))
            .Throws(new ArgumentException("invalid input"));

        var result = controller.Create(new Feedback { UserId = 0, OccurrenceId = 0 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_ReturnsServerError_OnUnexpectedException()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<Feedback>()))
            .Throws(new InvalidOperationException("boom"));

        var result = controller.Create(new Feedback { UserId = 1, OccurrenceId = 1 });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetAll_ReturnsOk_WhenListNotEmpty()
    {
        var list = new List<Feedback> { FeedbackFixture.Create(id: 1) };
        mockService.Setup(s => s.GetAllFeedbacks()).Returns(list);

        var res = controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.Same(list, ok.Value);
    }

    [Fact]
    public void GetAll_ReturnsNotFound_WhenEmpty()
    {
        mockService.Setup(s => s.GetAllFeedbacks()).Returns(new List<Feedback>());

        var res = controller.GetAll();

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenFound()
    {
        var fb = FeedbackFixture.Create(id: 5);
        mockService.Setup(s => s.GetFeedbackById(5)).Returns(fb);

        var res = controller.GetById(5);

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.Same(fb, ok.Value);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenMissing()
    {
        mockService.Setup(s => s.GetFeedbackById(It.IsAny<int>())).Returns((Feedback?)null);

        var res = controller.GetById(999);

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetById_ReturnsBadRequest_OnInvalidIdArgumentException()
    {
        mockService
            .Setup(s => s.GetFeedbackById(It.IsAny<int>()))
            .Throws(new ArgumentException("Id must be positive"));

        var res = controller.GetById(0);

        Assert.IsType<BadRequestObjectResult>(res);
    }

    [Fact]
    public void GetById_ReturnsServerError_OnUnexpectedException()
    {
        mockService
            .Setup(s => s.GetFeedbackById(It.IsAny<int>()))
            .Throws(new Exception("DB"));

        var res = controller.GetById(1);

        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetByUserId_ReturnsOk_WhenFound()
    {
        var list = new List<Feedback> { FeedbackFixture.Create(id: 2, userId: 10) };
        mockService.Setup(s => s.GetFeedbacksByUserId(10)).Returns(list);

        var res = controller.GetByUserId(10);

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.Same(list, ok.Value);
    }

    [Fact]
    public void GetByUserId_ReturnsNotFound_WhenEmpty()
    {
        mockService.Setup(s => s.GetFeedbacksByUserId(It.IsAny<int>())).Returns(new List<Feedback>());

        var res = controller.GetByUserId(1);

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByUserId_ReturnsBadRequest_OnInvalidId()
    {
        mockService
            .Setup(s => s.GetFeedbacksByUserId(It.IsAny<int>()))
            .Throws(new ArgumentException("Id must be positive"));

        var res = controller.GetByUserId(0);

        Assert.IsType<BadRequestObjectResult>(res);
    }

    [Fact]
    public void GetByOccurrenceId_ReturnsOk_WhenFound()
    {
        var list = new List<Feedback> { FeedbackFixture.Create(id: 3) };
        mockService.Setup(s => s.GetFeedbacksByOccurrenceId(7)).Returns(list);

        var res = controller.GetByOccurrenceId(7);

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.Same(list, ok.Value);
    }

    [Fact]
    public void GetByOccurrenceId_ReturnsNotFound_WhenEmpty()
    {
        mockService.Setup(s => s.GetFeedbacksByOccurrenceId(It.IsAny<int>())).Returns(new List<Feedback>());

        var res = controller.GetByOccurrenceId(3);

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByOccurrenceId_ReturnsBadRequest_OnInvalidId()
    {
        mockService
            .Setup(s => s.GetFeedbacksByOccurrenceId(It.IsAny<int>()))
            .Throws(new ArgumentException("Invalid id"));

        var res = controller.GetByOccurrenceId(0);

        Assert.IsType<BadRequestObjectResult>(res);
    }

    [Fact]
    public void GetAll_ServiceThrowsGeneric_ReturnsInternalServerError()
    {
        mockService.Setup(s => s.GetAllFeedbacks()).Throws(new Exception("db fail"));
        var res = controller.GetAll();
        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetByUserId_ServiceThrowsNotFoundMessage_ReturnsNotFound()
    {
        mockService
            .Setup(s => s.GetFeedbacksByUserId(It.IsAny<int>()))
            .Throws(new ArgumentException("User does not exist"));
        var res = controller.GetByUserId(1);
        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByUserId_ServerError_OnUnexpectedException()
    {
        mockService
            .Setup(s => s.GetFeedbacksByUserId(It.IsAny<int>()))
            .Throws(new Exception("boom"));
        var res = controller.GetByUserId(1);
        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetByOccurrenceId_ServiceThrowsNotFoundMessage_ReturnsNotFound()
    {
        mockService
            .Setup(s => s.GetFeedbacksByOccurrenceId(It.IsAny<int>()))
            .Throws(new ArgumentException("Occurrence does not exist"));
        var res = controller.GetByOccurrenceId(1);
        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByOccurrenceId_ServerError_OnUnexpectedException()
    {
        mockService
            .Setup(s => s.GetFeedbacksByOccurrenceId(It.IsAny<int>()))
            .Throws(new Exception("boom"));
        var res = controller.GetByOccurrenceId(1);
        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }
}
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


/// <summary>
///     This class contains all unit tests related to the Feedback API controller.
/// </summary>
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
    ///  Tests the Create method with valid feedback.
    /// </summary>
    [Fact]
    public void Create_ValidFeedback_Returns201WithPayload()
    {
        var fb = FeedbackFixture.Create(userId: 1, occurrenceId: 1);
        fb.Id = 10;
        mockService.Setup(s => s.Create(It.IsAny<Feedback>())).Returns(fb);

        var result = controller.Create(new Feedback { UserId = 1, OccurrenceId = 1 });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
        var payload = Assert.IsType<Feedback>(obj.Value);
        Assert.Equal(10, payload.Id);
    }

    /// <summary>
    ///   Tests the Create method when the service throws an exception indicating the occurrence is missing.
    /// </summary>
    [Fact]
    public void Create_ReturnsNotFound_WhenServiceThrowsOccurrenceMissing()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<Feedback>()))
            .Throws(new ArgumentException("Occurrence does not exist"));

        var result = controller.Create(new Feedback { UserId = 1, OccurrenceId = 999 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Create method when the service throws an ArgumentException.
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

    /// <summary>
    ///   Tests the Create method when the service throws an unexpected exception.
    /// </summary>
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
}
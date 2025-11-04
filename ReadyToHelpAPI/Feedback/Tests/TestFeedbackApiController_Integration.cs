namespace readytohelpapi.Feedback.Tests;

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Common.Data;
using readytohelpapi.Feedback.Controllers;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;
using readytohelpapi.Feedback.Tests.Fixtures;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.User.Models;
using Xunit;

/// <summary>
///   This class contains all integration tests related to the FeedbackApiController.
/// </summary>
[Trait("Category", "Integration")]
public class TestFeedbackApiController_Integration : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext context;
    private readonly FeedbackApiController controller;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestFeedbackApiController_Integration"/> class.
    /// </summary>
    /// <param name="fixture">The database fixture.</param>
    public TestFeedbackApiController_Integration(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        context = this.fixture.Context;

        var feedbackRepo = new FeedbackRepository(context);
        var occurrenceRepo = new OccurrenceRepository(context);

        var respSvc = new Mock<IResponsibleEntityService>();
        respSvc.Setup(s => s.FindResponsibleEntity(It.IsAny<OccurrenceType>(), It.IsAny<double>(), It.IsAny<double>()))
               .Returns((ResponsibleEntity.Models.ResponsibleEntity?)null);

        var occurrenceService = new OccurrenceServiceImpl(occurrenceRepo, respSvc.Object);
        var feedbackService = new FeedbackServiceImpl(feedbackRepo, occurrenceService);

        controller = new FeedbackApiController(feedbackService);
    }

    private User CreateUser()
    {
        var u = new User { Name = "U", Email = $"{Guid.NewGuid():N}@test.com", Password = "p", Profile = Profile.CITIZEN };
        context.Users.Add(u);
        context.SaveChanges();
        return u;
    }

    private Occurrence CreateOccurrence()
    {
        var occ = new Occurrence
        {
            Title = "Integration Occ",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.MEDIUM,
            ProximityRadius = 25,
            CreationDateTime = DateTime.UtcNow,
            ReportCount = 0,
            Location = new GeoPoint { Latitude = 41.0, Longitude = -8.0 }
        };
        context.Occurrences.Add(occ);
        context.SaveChanges();
        return occ;
    }

    /// <summary>
    ///   Tests the Create method persists the feedback in the database.
    /// </summary>
    [Fact]
    public void Create_PersistsFeedback_InDatabase()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();

        var result = controller.Create(new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
        var fb = Assert.IsType<Feedback>(obj.Value);
        Assert.True(fb.Id > 0);

        var inDb = context.Feedbacks.FirstOrDefault(f => f.Id == fb.Id);
        Assert.NotNull(inDb);
        Assert.Equal(user.Id, inDb!.UserId);
        Assert.Equal(occ.Id, inDb.OccurrenceId);
    }

    /// <summary>
    ///   Tests the Create method returns NotFound when the user does not exist.
    /// </summary>
    [Fact]
    public void Create_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var occ = CreateOccurrence();

        var res = controller.Create(new Feedback { UserId = 999999, OccurrenceId = occ.Id });

        Assert.IsType<NotFoundObjectResult>(res);
    }

    /// <summary>
    ///  Tests the Create method returns NotFound when the occurrence does not exist.
    /// </summary>
    [Fact]
    public void Create_ReturnsNotFound_WhenOccurrenceDoesNotExist()
    {
        var user = CreateUser();

        var res = controller.Create(new Feedback { UserId = user.Id, OccurrenceId = 999999 });

        Assert.IsType<NotFoundObjectResult>(res);
    }
}
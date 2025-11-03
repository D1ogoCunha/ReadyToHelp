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

[Trait("Category", "Integration")]
public class TestFeedbackApiController_Integration : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext context;
    private readonly FeedbackApiController controller;

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
            Location = new GeoPoint { Latitude = 41.0, Longitude = -8.0 },
            ReportId = null,
            ResponsibleEntityId = null
        };
        context.Occurrences.Add(occ);
        context.SaveChanges();
        return occ;
    }

    [Fact]
    public void Create_PersistsFeedback_InDatabase()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();

        var result = controller.Create(new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var fb = Assert.IsType<Feedback>(created.Value);
        Assert.True(fb.Id > 0);

        var inDb = context.Feedbacks.FirstOrDefault(f => f.Id == fb.Id);
        Assert.NotNull(inDb);
        Assert.Equal(user.Id, inDb!.UserId);
        Assert.Equal(occ.Id, inDb.OccurrenceId);
    }

    [Fact]
    public void Create_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var occ = CreateOccurrence();

        var res = controller.Create(new Feedback { UserId = 999999, OccurrenceId = occ.Id });

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void Create_ReturnsNotFound_WhenOccurrenceDoesNotExist()
    {
        var user = CreateUser();

        var res = controller.Create(new Feedback { UserId = user.Id, OccurrenceId = 999999 });

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenExists()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();
        var fb = new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow };
        context.Feedbacks.Add(fb);
        context.SaveChanges();

        var res = controller.GetById(fb.Id);

        var ok = Assert.IsType<OkObjectResult>(res);
        var payload = Assert.IsType<Feedback>(ok.Value);
        Assert.Equal(fb.Id, payload.Id);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenMissing()
    {
        var res = controller.GetById(123456);
        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetAll_ReturnsOk_WhenHasItems()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();
        context.Feedbacks.Add(new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow });
        context.SaveChanges();

        var res = controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void GetAll_ReturnsNotFound_WhenEmpty()
    {
        var res = controller.GetAll();
        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByUserId_ReturnsOk_ListForUser()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();
        context.Feedbacks.Add(new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow });
        context.SaveChanges();

        var res = controller.GetByUserId(user.Id);

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void GetByUserId_ReturnsNotFound_WhenNone()
    {
        var user = CreateUser();

        var res = controller.GetByUserId(user.Id);

        Assert.IsType<NotFoundObjectResult>(res);
    }

    [Fact]
    public void GetByOccurrenceId_ReturnsOk_ListForOccurrence()
    {
        var user = CreateUser();
        var occ = CreateOccurrence();
        context.Feedbacks.Add(new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow });
        context.SaveChanges();

        var res = controller.GetByOccurrenceId(occ.Id);

        var ok = Assert.IsType<OkObjectResult>(res);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void GetByOccurrenceId_ReturnsNotFound_WhenNone()
    {
        var occ = CreateOccurrence();

        var res = controller.GetByOccurrenceId(occ.Id);

        Assert.IsType<NotFoundObjectResult>(res);
    }
}
namespace readytohelpapi.Feedback.Tests;

using System;
using readytohelpapi.Common.Data;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;
using readytohelpapi.User.Models;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;
using readytohelpapi.Feedback.Tests.Fixtures;
using readytohelpapi.ResponsibleEntity.Models;
using Xunit;

/// <summary>
///  This class contains all integration tests related to the feedback repository.
/// </summary>
public class TestFeedbackRepository : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext context;
    private readonly IFeedbackRepository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFeedbackRepository"/> class.
    /// </summary>
    public TestFeedbackRepository(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        this.context = this.fixture.Context;
        this.repository = new FeedbackRepository(this.context);
    }

    private int CreateResponsibleEntityHelper()
    {
        var re = new ResponsibleEntity
        {
            Name = $"TestRE-{Guid.NewGuid():N}",
            Email = "re@example.com",
            Address = "addr",
            ContactPhone = 123456789,
            Type = ResponsibleEntityType.INEM
        };
        this.context.Set<ResponsibleEntity>().Add(re);
        this.context.SaveChanges();
        return re.Id;
    }
    /// <summary>
    /// Tests the Create method with null feedback.
    /// </summary>
    [Fact]
    public void Create_NullFeedback_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => this.repository.Create(null!));
    }

    /// <summary>
    /// Tests the Create method with valid feedback.
    /// </summary>
    [Fact]
    public void Create_ValidFeedback_ReturnsCreatedFeedback()
    {
        var user = new User { Name = "u", Email = "u@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r1",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();

        var occ = new Occurrence
        {
            Title = "o",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb = new Feedback { UserId = user.Id, OccurrenceId = occ.Id, IsConfirmed = true };

        var created = this.repository.Create(fb);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(user.Id, created.UserId);
        Assert.Equal(occ.Id, created.OccurrenceId);
    }

    /// <summary>
    /// Tests the GetFeedbackById method when feedback exists.
    /// </summary>
    [Fact]
    public void GetFeedbackById_ReturnsFeedback_WhenExists()
    {
        var user = new User { Name = "u2", Email = "u2@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r2",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();

        var occ = new Occurrence
        {
            Title = "o2",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        this.context.Feedbacks.Add(fb);
        this.context.SaveChanges();

        var result = this.repository.GetFeedbackById(fb.Id);

        Assert.NotNull(result);
        Assert.Equal(fb.Id, result.Id);
    }

    /// <summary>
    /// Tests the GetFeedbackById method when feedback does not exist.
    /// </summary>
    [Fact]
    public void GetFeedbackById_ReturnsNull_WhenNotFound()
    {
        var res = this.repository.GetFeedbackById(999999);
        Assert.Null(res);
    }

    /// <summary>
    /// Tests the GetAllFeedbacks method.
    /// </summary>
    [Fact]
    public void GetAllFeedbacks_ReturnsList()
    {
        var user = new User { Name = "u3", Email = "u3@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r3",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();

        var occ = new Occurrence
        {
            Title = "o3",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        this.context.Feedbacks.Add(fb);
        this.context.SaveChanges();

        var list = this.repository.GetAllFeedbacks();

        Assert.NotNull(list);
        Assert.True(list.Count >= 1);
    }

    /// <summary>
    /// Tests the GetFeedbacksByOccurrenceId method.
    /// </summary>
    [Fact]
    public void GetFeedbacksByOccurrenceId_ReturnsList()
    {
        var user = new User { Name = "u4", Email = "u4@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r4",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();
        var occ = new Occurrence
        {
            Title = "o4",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb1 = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        var fb2 = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        this.context.Feedbacks.AddRange(fb1, fb2);
        this.context.SaveChanges();

        var res = this.repository.GetFeedbacksByOccurrenceId(occ.Id);

        Assert.NotNull(res);
        Assert.True(res.Count >= 2);
        Assert.All(res, f => Assert.Equal(occ.Id, f.OccurrenceId));
    }

    /// <summary>
    /// Tests the GetFeedbacksByUserId method.
    /// </summary>
    [Fact]
    public void GetFeedbacksByUserId_ReturnsList()
    {
        var user = new User { Name = "u5", Email = "u5@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r5",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();
        
        var occ = new Occurrence
        {
            Title = "o5",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb1 = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        var fb2 = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        this.context.Feedbacks.AddRange(fb1, fb2);
        this.context.SaveChanges();

        var res = this.repository.GetFeedbacksByUserId(user.Id);

        Assert.NotNull(res);
        Assert.True(res.Count >= 2);
        Assert.All(res, f => Assert.Equal(user.Id, f.UserId));
    }

    /// <summary>
    /// Tests the UserExists and OccurrenceExists methods.
    /// </summary>
    [Fact]
    public void UserExists_And_OccurrenceExists_Work()
    {
        var user = new User { Name = "u6", Email = "u6@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r6",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();

        var occ = new Occurrence
        {
            Title = "o6",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        Assert.True(this.repository.UserExists(user.Id));
        Assert.True(this.repository.OccurrenceExists(occ.Id));

        Assert.False(this.repository.UserExists(99999));
        Assert.False(this.repository.OccurrenceExists(99999));
    }

    /// <summary>
    /// Tests the Delete method.
    /// </summary>
    [Fact]
    public void Delete_ExistingFeedback_RemovesIt()
    {
        var user = new User { Name = "u7", Email = "u7@example.com", Password = "p", Profile = Profile.CITIZEN };
        this.context.Users.Add(user);
        this.context.SaveChanges();

        var report = new Report
        {
            Title = "r7",
            Description = "report desc",
            UserId = user.Id,
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 }
        };
        this.context.Reports.Add(report);
        this.context.SaveChanges();

        var reId = CreateResponsibleEntityHelper();

        var occ = new Occurrence
        {
            Title = "o7",
            Description = "teste ocorrencia",
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
            Location = new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 },
            ReportId = report.Id,
            ResponsibleEntityId = reId
        };
        this.context.Occurrences.Add(occ);
        this.context.SaveChanges();

        var fb = new Feedback { UserId = user.Id, OccurrenceId = occ.Id };
        this.context.Feedbacks.Add(fb);
        this.context.SaveChanges();

        var toRemove = this.context.Feedbacks.Find(fb.Id);
        this.context.Feedbacks.Remove(toRemove!);
        this.context.SaveChanges();

        var got = this.repository.GetFeedbackById(fb.Id);
        Assert.Null(got);
    }
}

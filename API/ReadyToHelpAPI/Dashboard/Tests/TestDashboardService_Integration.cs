namespace readytohelpapi.Dashboard.Tests;

using System;
using System.Threading.Tasks;
using readytohelpapi.Common.Data;
using readytohelpapi.Dashboard.Service;
using readytohelpapi.Dashboard.Tests.Fixtures;
using readytohelpapi.Feedback.Models;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.User.Models;
using Xunit;

/// <summary>
/// This class contains all integration tests for DashboardServiceImpl.
/// </summary>
[Trait("Category", "Integration")]
public class TestDashboardService_Integration : IClassFixture<DbFixture>
{
    private readonly AppDbContext ctx;
    private readonly DashboardServiceImpl svc;
    private readonly DbFixture fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDashboardService_Integration_Integration"/> class.
    /// </summary>
    public TestDashboardService_Integration(DbFixture fixture)
    {
        this.fixture = fixture;
        fixture.ResetDatabase();
        ctx = fixture.Context;
        svc = new DashboardServiceImpl(ctx);
    }

    /// <summary>
    /// Tests the GetOccurrenceStatsAsync method to ensure the average resolution is correct.
    /// </summary>
    [Fact]
    public async Task GetOccurrenceStatsAsync_AverageResolution()
    {
        ctx.Occurrences.Add(
            new Occurrence
            {
                Title = "occA",
                CreationDateTime = DateTime.UtcNow.AddHours(-10),
                EndDateTime = DateTime.UtcNow.AddHours(-5),
                Priority = PriorityLevel.HIGH,
                Status = OccurrenceStatus.CLOSED,
                ReportCount = 2,
            }
        );
        ctx.Occurrences.Add(
            new Occurrence
            {
                Title = "occB",
                CreationDateTime = DateTime.UtcNow.AddHours(-6),
                EndDateTime = DateTime.UtcNow.AddHours(-2),
                Priority = PriorityLevel.MEDIUM,
                Status = OccurrenceStatus.CLOSED,
                ReportCount = 3,
            }
        );
        await ctx.SaveChangesAsync();

        var stats = await svc.GetOccurrenceStatsAsync();

        Assert.InRange(stats.AverageResolutionHours, 4.0, 5.0);
        Assert.Equal(2, stats.TotalOccurrences);
    }

    /// <summary>
    /// Tests the GetReportStatsAsync method to ensure counts and top reporter are correct.
    /// </summary>
    [Fact]
    public async Task GetReportStatsAsync_ReturnsCountsAndTopReporter()
    {
        fixture.ResetDatabase();

        var now = DateTime.UtcNow;
        var u1 = new User
        {
            Name = "Alice",
            Email = "a@x",
            Password = "p",
        };
        var u2 = new User
        {
            Name = "Bob",
            Email = "b@x",
            Password = "p",
        };
        ctx.Users.AddRange(u1, u2);
        await ctx.SaveChangesAsync();

        ctx.Reports.AddRange(
            new Report
            {
                UserId = u1.Id,
                ReportDateTime = now.AddDays(-1),
                Type = OccurrenceType.ROAD_DAMAGE,
                Title = "r1",
                Description = "d1",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            },
            new Report
            {
                UserId = u1.Id,
                ReportDateTime = now.AddDays(-2),
                Type = OccurrenceType.ROAD_DAMAGE,
                Title = "r2",
                Description = "d2",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            },
            new Report
            {
                UserId = u2.Id,
                ReportDateTime = now.AddDays(-3),
                Type = OccurrenceType.FLOOD,
                Title = "r3",
                Description = "d3",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            }
        );
        await ctx.SaveChangesAsync();

        var stats = await svc.GetReportStatsAsync();

        Assert.Equal(3, stats.TotalReports);
        Assert.Equal(2, stats.ReportsByType["ROAD_DAMAGE"]);
        Assert.Equal(1, stats.ReportsByType["FLOOD"]);
        Assert.Equal(u1.Id, stats.TopReporterUserId);
        Assert.Equal("Alice", stats.TopReporterUserName);
        Assert.Equal(2, stats.TopReporterReportCount);
    }

    /// <summary>
    /// Tests the GetFeedbackStatsAsync method to ensure confirmation rate and top user are computed correctly.
    /// </summary>
    [Fact]
    public async Task GetFeedbackStatsAsync_ComputesConfirmationAndTopUser()
    {
        fixture.ResetDatabase();

        var u1 = new User
        {
            Name = "U1",
            Email = "u1@x",
            Password = "p",
        };
        var u2 = new User
        {
            Name = "U2",
            Email = "u2@x",
            Password = "p",
        };
        ctx.Users.AddRange(u1, u2);
        await ctx.SaveChangesAsync();

        var occ = new Occurrence
        {
            Title = "fb_occ",
            CreationDateTime = DateTime.UtcNow,
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
        };
        ctx.Occurrences.Add(occ);
        await ctx.SaveChangesAsync();

        ctx.Feedbacks.AddRange(
            new Feedback
            {
                UserId = u1.Id,
                OccurrenceId = occ.Id,
                IsConfirmed = true,
                FeedbackDateTime = DateTime.UtcNow.AddDays(-1),
            },
            new Feedback
            {
                UserId = u1.Id,
                OccurrenceId = occ.Id,
                IsConfirmed = false,
                FeedbackDateTime = DateTime.UtcNow.AddDays(-2),
            },
            new Feedback
            {
                UserId = u2.Id,
                OccurrenceId = occ.Id,
                IsConfirmed = true,
                FeedbackDateTime = DateTime.UtcNow.AddDays(-3),
            }
        );
        await ctx.SaveChangesAsync();

        var stats = await svc.GetFeedbackStatsAsync();

        Assert.Equal(3, stats.TotalFeedbacks);
        Assert.Equal(2, stats.ConfirmedCount);
        Assert.Equal(u1.Id, stats.TopFeedbackUserId);
        Assert.Equal("U1", stats.TopFeedbackUserName);
        Assert.InRange(stats.ConfirmationRate, 60.0, 67.0);
    }

    /// <summary>
    /// Tests the GetUserStatsAsync method to ensure it returns the correct user breakdown and activity.
    /// </summary>
    [Fact]
    public async Task GetUserStatsAsync_ReturnsBreakdownAndActivity()
    {
        fixture.ResetDatabase();

        var admin = new User
        {
            Name = "Admin",
            Email = "a@x",
            Password = "p",
            Profile = Profile.ADMIN,
        };
        var manager = new User
        {
            Name = "Manager",
            Email = "m@x",
            Password = "p",
            Profile = Profile.MANAGER,
        };
        var c1 = new User
        {
            Name = "C1",
            Email = "c1@x",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        var c2 = new User
        {
            Name = "C2",
            Email = "c2@x",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        ctx.Users.AddRange(admin, manager, c1, c2);
        await ctx.SaveChangesAsync();

        var occ = new Occurrence
        {
            Title = "userstats_occ",
            CreationDateTime = DateTime.UtcNow,
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.WAITING,
        };
        ctx.Occurrences.Add(occ);
        await ctx.SaveChangesAsync();

        ctx.Reports.AddRange(
            new Report
            {
                UserId = c1.Id,
                Title = "r1",
                Description = "d",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            },
            new Report
            {
                UserId = c1.Id,
                Title = "r2",
                Description = "d",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            },
            new Report
            {
                UserId = c2.Id,
                Title = "r3",
                Description = "d",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            }
        );
        ctx.Feedbacks.AddRange(
            new Feedback
            {
                UserId = c1.Id,
                OccurrenceId = occ.Id,
                FeedbackDateTime = DateTime.UtcNow,
            },
            new Feedback
            {
                UserId = c2.Id,
                OccurrenceId = occ.Id,
                FeedbackDateTime = DateTime.UtcNow,
            }
        );
        await ctx.SaveChangesAsync();

        var stats = await svc.GetUserStatsAsync();

        Assert.Equal(4, stats.TotalUsers);
        Assert.Equal(1, stats.Admins);
        Assert.Equal(1, stats.Managers);
        Assert.Equal(2, stats.Citizens);
        Assert.Equal(2, stats.UsersWithReports);
        Assert.Equal(2, stats.UsersWithFeedbacks);
        Assert.True(stats.UsersWithBoth >= 1);
    }

    /// <summary>
    /// Tests the GetResponsibleEntityStatsAsync method to ensure it returns correct assignments and top entity.
    /// </summary>
    [Fact]
    public async Task GetResponsibleEntityStatsAsync_ReturnsAssignmentsAndTopEntity()
    {
        fixture.ResetDatabase();

        var re1 = new ResponsibleEntity
        {
            Name = "E1",
            Email = "e1@x",
            Type = ResponsibleEntityType.POLICIA,
        };
        var re2 = new ResponsibleEntity
        {
            Name = "E2",
            Email = null,
            Type = ResponsibleEntityType.BOMBEIROS,
        };
        ctx.ResponsibleEntities.AddRange(re1, re2);
        await ctx.SaveChangesAsync();

        ctx.Occurrences.AddRange(
            new Occurrence
            {
                ResponsibleEntityId = re1.Id,
                Status = OccurrenceStatus.ACTIVE,
                Type = OccurrenceType.ROAD_DAMAGE,
            },
            new Occurrence
            {
                ResponsibleEntityId = re1.Id,
                Status = OccurrenceStatus.WAITING,
                Type = OccurrenceType.FLOOD,
            },
            new Occurrence
            {
                ResponsibleEntityId = re2.Id,
                Status = OccurrenceStatus.CLOSED,
                Type = OccurrenceType.FLOOD,
            }
        );
        await ctx.SaveChangesAsync();

        var stats = await svc.GetResponsibleEntityStatsAsync();

        Assert.Equal(2, stats.TotalResponsibleEntities);
        Assert.True(stats.TotalAssignedOccurrences >= 3);
        Assert.True(stats.TopEntityByOccurrencesCount >= 1);
        Assert.True(stats.EntitiesWithContactInfo >= 1);
    }

    /// <summary>
    /// Tests the GetOverviewAsync method to ensure it returns aggregated totals.
    /// </summary>
    [Fact]
    public async Task GetOverviewAsync_ReturnsAggregatedTotals()
    {
        fixture.ResetDatabase();

        var u = new User
        {
            Name = "overview_user",
            Email = "o@x",
            Password = "p",
        };
        ctx.Users.Add(u);
        await ctx.SaveChangesAsync();

        var occ = new Occurrence
        {
            Title = "overview_occ",
            CreationDateTime = DateTime.UtcNow,
            Type = OccurrenceType.ROAD_DAMAGE,
            Status = OccurrenceStatus.WAITING,
        };
        ctx.Occurrences.Add(occ);
        await ctx.SaveChangesAsync();

        ctx.Reports.Add(
            new Report
            {
                UserId = u.Id,
                Title = "r",
                Description = "d",
                Location = new GeoPoint { Latitude = 0, Longitude = 0 },
            }
        );
        ctx.Feedbacks.Add(
            new Feedback
            {
                UserId = u.Id,
                OccurrenceId = occ.Id,
                IsConfirmed = true,
                FeedbackDateTime = DateTime.UtcNow,
            }
        );
        ctx.ResponsibleEntities.Add(
            new ResponsibleEntity
            {
                Name = "re",
                Email = "re@x",
                Type = ResponsibleEntityType.INEM,
            }
        );
        ctx.Occurrences.Add(
            new Occurrence
            {
                Title = "occ",
                CreationDateTime = DateTime.UtcNow,
                Type = OccurrenceType.ROAD_DAMAGE,
                Status = OccurrenceStatus.WAITING,
            }
        );
        await ctx.SaveChangesAsync();

        var dto = await svc.GetOverviewAsync();

        Assert.True(dto.TotalUsers >= 1);
        Assert.True(dto.TotalReports >= 1);
        Assert.True(dto.TotalFeedbacks >= 1);
        Assert.True(dto.TotalResponsibleEntities >= 1);
        Assert.True(dto.TotalOccurrences >= 1);
    }
}

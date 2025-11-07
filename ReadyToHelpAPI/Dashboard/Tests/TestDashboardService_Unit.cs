namespace readytohelpapi.Dashboard.Tests;

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Dashboard.Service;
using readytohelpapi.Feedback.Models;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.User.Models;
using Xunit;

[Trait("Category", "Unit")]
public class TestDashboardService_Unit
{
    private static (DashboardServiceImpl svc, AppDbContext ctx) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"unit_dashboard_{Guid.NewGuid():N}")
            .Options;

        var ctx = new AppDbContext(options);
        var svc = new DashboardServiceImpl(ctx);
        return (svc, ctx);
    }

    private static DateTime UtcDaysAgo(int days) => DateTime.UtcNow.AddDays(-days);
    private static DateTime UtcHoursAgo(int hours) => DateTime.UtcNow.AddHours(-hours);

    private static User AddUser(AppDbContext ctx, string name, Profile profile)
    {
        var u = new User { Name = name, Email = $"{Guid.NewGuid():N}@t.local", Password = "x", Profile = profile };
        ctx.Users.Add(u);
        ctx.SaveChanges();
        return u;
    }

    private static Report AddReport(AppDbContext ctx, int userId, OccurrenceType type, DateTime when)
    {
        var r = new Report
        {
            Title = "R",
            Description = "D",
            UserId = userId,
            Type = type,
            ReportDateTime = when,
            Location = new GeoPoint { Latitude = 41.15, Longitude = -8.61 }
        };
        ctx.Reports.Add(r);
        ctx.SaveChanges();
        return r;
    }

    private static void AddFeedback(AppDbContext ctx, int userId, bool confirmed, DateTime when, int occurrenceId)
    {
        var f = new Feedback
        {
            OccurrenceId = occurrenceId,
            UserId = userId,
            IsConfirmed = confirmed,
            FeedbackDateTime = when
        };
        ctx.Feedbacks.Add(f);
        ctx.SaveChanges();
    }

    private static Occurrence AddOccurrence(
        AppDbContext ctx,
        string title,
        OccurrenceStatus status,
        PriorityLevel prio,
        OccurrenceType type,
        int responsibleId,
        DateTime? created = null,
        DateTime? ended = null,
        int reportCount = 0)
    {
        var o = new Occurrence
        {
            Title = title,
            Status = status,
            Priority = prio,
            Type = type,
            ReportCount = reportCount,
            CreationDateTime = created ?? DateTime.UtcNow,
            EndDateTime = ended ?? DateTime.UtcNow.AddDays(1),
            ResponsibleEntityId = responsibleId,
            Location = new GeoPoint { Latitude = 41.15, Longitude = -8.61 }
        };
        ctx.Occurrences.Add(o);
        ctx.SaveChanges();
        return o;
    }

    private static ResponsibleEntity AddResponsibleEntity(AppDbContext ctx, string name, ResponsibleEntityType type, string? email = "e@x")
    {
        var re = new ResponsibleEntity { Name = name, Type = type, Email = email, Address = "addr", ContactPhone = 123 };
        ctx.ResponsibleEntities.Add(re);
        ctx.SaveChanges();
        return re;
    }

    [Fact]
    public async Task GetOverviewAsync_Returns_All_Totals()
    {
        var (svc, ctx) = CreateService();

        var u1 = AddUser(ctx, "A", Profile.ADMIN);
        var u2 = AddUser(ctx, "B", Profile.CITIZEN);

        AddReport(ctx, u1.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(1));
        AddReport(ctx, u2.Id, OccurrenceType.FLOOD, UtcDaysAgo(10));

        var re1 = AddResponsibleEntity(ctx, "RE1", ResponsibleEntityType.BOMBEIROS);
        var re2 = AddResponsibleEntity(ctx, "RE2", ResponsibleEntityType.POLICIA);

        var o1 = AddOccurrence(ctx, "O1", OccurrenceStatus.WAITING, PriorityLevel.HIGH, OccurrenceType.ROAD_DAMAGE, re1.Id);
        var o2 = AddOccurrence(ctx, "O2", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.FLOOD, re2.Id);

        AddFeedback(ctx, u1.Id, true, UtcDaysAgo(2), o1.Id);
        AddFeedback(ctx, u2.Id, false, UtcDaysAgo(1), o2.Id);

        var dto = await svc.GetOverviewAsync();

        Assert.Equal(2, dto.TotalUsers);
        Assert.Equal(2, dto.TotalReports);
        Assert.Equal(2, dto.TotalFeedbacks);
        Assert.Equal(2, dto.TotalResponsibleEntities);
        Assert.Equal(2, dto.TotalOccurrences);
    }

    [Fact]
    public async Task GetUserStatsAsync_Computes_RoleCounts_Activity_And_MostActive()
    {
        var (svc, ctx) = CreateService();

        var admin = AddUser(ctx, "Admin", Profile.ADMIN);
        AddUser(ctx, "Manager", Profile.MANAGER);
        AddUser(ctx, "Citizen1", Profile.CITIZEN);
        var citizen2 = AddUser(ctx, "Citizen2", Profile.CITIZEN);
        var re = AddResponsibleEntity(ctx, "RE", ResponsibleEntityType.BOMBEIROS);
        var occ = AddOccurrence(ctx, "O", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.FLOOD, re.Id);

        AddReport(ctx, admin.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(1));
        AddReport(ctx, admin.Id, OccurrenceType.FLOOD, UtcDaysAgo(2));
        AddReport(ctx, citizen2.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(3));

        AddFeedback(ctx, admin.Id, true, UtcDaysAgo(1), occ.Id);
        AddFeedback(ctx, citizen2.Id, false, UtcDaysAgo(2), occ.Id);
        AddFeedback(ctx, citizen2.Id, true, UtcDaysAgo(10), occ.Id);
        AddFeedback(ctx, citizen2.Id, true, UtcHoursAgo(3), occ.Id);

        var dto = await svc.GetUserStatsAsync();

        Assert.Equal(4, dto.TotalUsers);
        Assert.Equal(1, dto.Admins);
        Assert.Equal(1, dto.Managers);
        Assert.Equal(2, dto.Citizens);

        Assert.Equal(2, dto.UsersWithReports);
        Assert.Equal(2, dto.UsersWithFeedbacks);
        Assert.Equal(2, dto.UsersWithBoth);
        Assert.Equal(2, dto.UsersWithoutReportsOrFeedbacks);

        Assert.Equal(citizen2.Id, dto.MostActiveUserId);
        Assert.Equal("Citizen2", dto.MostActiveUserName);
        Assert.Equal(1, dto.MostActiveUserReports);
        Assert.Equal(3, dto.MostActiveUserFeedbacks);
    }

    [Fact]
    public async Task GetReportStatsAsync_Computes_Windows_TopReporter_ByType_And_Dates()
    {
        var (svc, ctx) = CreateService();

        var u1 = AddUser(ctx, "U1", Profile.CITIZEN);
        var u2 = AddUser(ctx, "U2", Profile.CITIZEN);

        var r1 = AddReport(ctx, u1.Id, OccurrenceType.ROAD_DAMAGE, UtcHoursAgo(5));
        AddReport(ctx, u1.Id, OccurrenceType.FLOOD, UtcDaysAgo(3));
        AddReport(ctx, u1.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(10));
        var r4 = AddReport(ctx, u2.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(35));

        var dto = await svc.GetReportStatsAsync();

        Assert.Equal(4, dto.TotalReports);
        Assert.Equal(1, dto.NewReportsLast24Hours);
        Assert.Equal(2, dto.NewReportsLast7Days);
        Assert.Equal(3, dto.NewReportsLast30Days);
        Assert.Equal(2, dto.UniqueReporters);
        Assert.Equal(Math.Round(3 / 30.0, 2), dto.AverageReportsPerDayLast30);

        Assert.Equal(3, dto.ReportsByType[nameof(OccurrenceType.ROAD_DAMAGE)]);
        Assert.Equal(1, dto.ReportsByType[nameof(OccurrenceType.FLOOD)]);

        Assert.Equal(u1.Id, dto.TopReporterUserId);
        Assert.Equal("U1", dto.TopReporterUserName);
        Assert.Equal(3, dto.TopReporterReportCount);

        Assert.NotNull(dto.FirstReportDate);
        Assert.NotNull(dto.LastReportDate);

        Assert.True(Math.Abs((r4.ReportDateTime - dto.FirstReportDate.Value).TotalSeconds) < 1);
        Assert.True(Math.Abs((r1.ReportDateTime - dto.LastReportDate.Value).TotalSeconds) < 1);
    }

    [Fact]
    public async Task GetFeedbackStatsAsync_Computes_Windows_Rates_TopUser_And_Dates()
    {
        var (svc, ctx) = CreateService();

        var u1 = AddUser(ctx, "A", Profile.CITIZEN);
        var u2 = AddUser(ctx, "B", Profile.CITIZEN);
        var re = AddResponsibleEntity(ctx, "RE", ResponsibleEntityType.BOMBEIROS);
        var occ = AddOccurrence(ctx, "O", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.FLOOD, re.Id);

        AddFeedback(ctx, u1.Id, true, UtcHoursAgo(3), occ.Id);
        AddFeedback(ctx, u1.Id, false, UtcDaysAgo(2), occ.Id);
        AddFeedback(ctx, u1.Id, true, UtcDaysAgo(8), occ.Id);
        AddFeedback(ctx, u2.Id, true, UtcHoursAgo(23), occ.Id);
        AddFeedback(ctx, u2.Id, false, UtcDaysAgo(40), occ.Id);

        var dto = await svc.GetFeedbackStatsAsync();

        Assert.Equal(5, dto.TotalFeedbacks);
        Assert.Equal(2, dto.NewFeedbacksLast24Hours);
        Assert.Equal(3, dto.NewFeedbacksLast7Days);
        Assert.Equal(4, dto.NewFeedbacksLast30Days);
        Assert.Equal(2, dto.UniqueUsers);

        Assert.Equal(3, dto.ConfirmedCount);
        Assert.Equal(2, dto.NotConfirmedCount);
        Assert.Equal(Math.Round(3.0 / 5.0 * 100.0, 2), dto.ConfirmationRate);

        Assert.Equal(Math.Round(4.0 / 30.0, 2), dto.AverageFeedbacksPerDayLast30);

        Assert.Equal(u1.Id, dto.TopFeedbackUserId);
        Assert.Equal("A", dto.TopFeedbackUserName);
        Assert.Equal(3, dto.TopFeedbackUserCount);

        Assert.NotNull(dto.FirstFeedbackDate);
        Assert.NotNull(dto.LastFeedbackDate);
        Assert.True(dto.FirstFeedbackDate < dto.LastFeedbackDate);
    }

    [Fact]
    public async Task GetResponsibleEntityStatsAsync_Computes_ByType_Assignments_Active_And_Top()
    {
        var (svc, ctx) = CreateService();

        var re1 = AddResponsibleEntity(ctx, "RE1", ResponsibleEntityType.BOMBEIROS, email: "a@x");
        var re2 = AddResponsibleEntity(ctx, "RE2", ResponsibleEntityType.POLICIA, email: "");
        AddResponsibleEntity(ctx, "RE3", ResponsibleEntityType.BOMBEIROS, email: null);

        AddOccurrence(ctx, "o1", OccurrenceStatus.WAITING, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, responsibleId: re1.Id);
        AddOccurrence(ctx, "o2", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, responsibleId: re1.Id);
        AddOccurrence(ctx, "o3", OccurrenceStatus.CLOSED, PriorityLevel.MEDIUM, OccurrenceType.FLOOD, responsibleId: re2.Id);

        var dto = await svc.GetResponsibleEntityStatsAsync();

        Assert.Equal(3, dto.TotalResponsibleEntities);
        Assert.Equal(2, dto.ByType[nameof(ResponsibleEntityType.BOMBEIROS)]);
        Assert.Equal(1, dto.ByType[nameof(ResponsibleEntityType.POLICIA)]);

        Assert.Equal(2, dto.EntitiesWithAssignedOccurrences);
        Assert.Equal(1, dto.EntitiesWithoutAssignedOccurrences);
        Assert.Equal(3, dto.TotalAssignedOccurrences);
        Assert.Equal(1.0, dto.AverageOccurrencesPerEntity);

        Assert.Equal(2, dto.ActiveOccurrences);

        Assert.Equal(1, dto.OccurrencesByStatus[nameof(OccurrenceStatus.WAITING)]);
        Assert.Equal(1, dto.OccurrencesByStatus[nameof(OccurrenceStatus.ACTIVE)]);
        Assert.Equal(1, dto.OccurrencesByStatus[nameof(OccurrenceStatus.CLOSED)]);

        Assert.Equal(1, dto.EntitiesWithContactInfo);
        Assert.Equal(2, dto.EntitiesWithoutContactInfo);

        Assert.Equal(re1.Id, dto.TopEntityByOccurrencesId);
        Assert.Equal("RE1", dto.TopEntityByOccurrencesName);
        Assert.Equal(2, dto.TopEntityByOccurrencesCount);
    }

    [Fact]
    public async Task GetOverviewAsync_EmptyDatabase_ReturnsZeros()
    {
        var (svc, _) = CreateService();

        var dto = await svc.GetOverviewAsync();

        Assert.Equal(0, dto.TotalUsers);
        Assert.Equal(0, dto.TotalReports);
        Assert.Equal(0, dto.TotalFeedbacks);
        Assert.Equal(0, dto.TotalResponsibleEntities);
        Assert.Equal(0, dto.TotalOccurrences);
    }

    [Fact]
    public async Task GetOccurrenceStatsAsync_MostReported_TieBreaks_ByCreationDateDesc()
    {
        var (svc, ctx) = CreateService();

        var re = AddResponsibleEntity(ctx, "RE", ResponsibleEntityType.BOMBEIROS);

        AddOccurrence(ctx, "Older", OccurrenceStatus.CLOSED, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, re.Id, created: UtcDaysAgo(10), ended: UtcDaysAgo(9), reportCount: 5);
        AddOccurrence(ctx, "Newer", OccurrenceStatus.CLOSED, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, re.Id, created: UtcDaysAgo(1), ended: UtcDaysAgo(0), reportCount: 5);
        AddOccurrence(ctx, "A", OccurrenceStatus.ACTIVE, PriorityLevel.MEDIUM, OccurrenceType.FLOOD, re.Id, created: UtcDaysAgo(2), ended: default);
        AddOccurrence(ctx, "W", OccurrenceStatus.WAITING, PriorityLevel.HIGH, OccurrenceType.ROAD_DAMAGE, re.Id, created: UtcDaysAgo(3), ended: default);

        var dto = await svc.GetOccurrenceStatsAsync();

        Assert.Equal("Newer", dto.MostReportedOccurrenceTitle);
        Assert.Equal(5, dto.MostReports);
    }

    [Fact]
    public async Task GetReportStatsAsync_TopReporter_TieBreaks_ByUserIdAscending()
    {
        var (svc, ctx) = CreateService();

        var uSmallId = AddUser(ctx, "Small", Profile.CITIZEN);
        var uBigId = AddUser(ctx, "Big", Profile.CITIZEN);

        AddReport(ctx, uSmallId.Id, OccurrenceType.ROAD_DAMAGE, UtcDaysAgo(1));
        AddReport(ctx, uBigId.Id, OccurrenceType.FLOOD, UtcDaysAgo(2));

        var dto = await svc.GetReportStatsAsync();

        Assert.Equal(1, dto.ReportsByType[nameof(OccurrenceType.ROAD_DAMAGE)]);
        Assert.Equal(1, dto.ReportsByType[nameof(OccurrenceType.FLOOD)]);
        Assert.Equal(uSmallId.Id, dto.TopReporterUserId);
        Assert.Equal("Small", dto.TopReporterUserName);
        Assert.Equal(1, dto.TopReporterReportCount);
    }

    [Fact]
    public async Task GetResponsibleEntityStatsAsync_TopEntity_WhenTie_ReturnsEitherEntity()
    {
        var (svc, ctx) = CreateService();

        var re1 = AddResponsibleEntity(ctx, "E1", ResponsibleEntityType.BOMBEIROS);
        var re2 = AddResponsibleEntity(ctx, "E2", ResponsibleEntityType.BOMBEIROS);

        AddOccurrence(ctx, "o1", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, re1.Id, created: UtcDaysAgo(1), ended: default);
        AddOccurrence(ctx, "o2", OccurrenceStatus.ACTIVE, PriorityLevel.LOW, OccurrenceType.ROAD_DAMAGE, re2.Id, created: UtcDaysAgo(1), ended: default);

        var dto = await svc.GetResponsibleEntityStatsAsync();

        Assert.Equal(2, dto.TotalResponsibleEntities);
        Assert.True(dto.TopEntityByOccurrencesId == re1.Id || dto.TopEntityByOccurrencesId == re2.Id);
        Assert.Equal(1, dto.TopEntityByOccurrencesCount);
    }

    [Fact]
    public async Task GetOccurrenceStatsAsync_AverageResolutionHours_ComputesCorrectAverage()
    {
        var (svc, ctx) = CreateService();

        var re = AddResponsibleEntity(ctx, "RE", ResponsibleEntityType.BOMBEIROS);

        AddOccurrence(ctx, "r1", OccurrenceStatus.CLOSED, PriorityLevel.LOW, OccurrenceType.FLOOD, re.Id,
            created: DateTime.UtcNow.AddHours(-2), ended: DateTime.UtcNow.AddHours(-1));
        AddOccurrence(ctx, "r2", OccurrenceStatus.CLOSED, PriorityLevel.LOW, OccurrenceType.FLOOD, re.Id,
            created: DateTime.UtcNow.AddHours(-3), ended: DateTime.UtcNow.AddHours(-1));

        var dto = await svc.GetOccurrenceStatsAsync();
        Assert.Equal(1.5, dto.AverageResolutionHours);
    }
}
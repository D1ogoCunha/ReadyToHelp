namespace readytohelpapi.Report.Tests;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Notifications;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Tests.Fixtures;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

/// <summary>
/// This class contains all integration tests for ReportServiceImpl.
/// </summary>
[Trait("Category", "Integration")]
public class TestReportService_Integration : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext context;
    private readonly ReportRepository reportRepo;
    private readonly OccurrenceRepository occurrenceRepo;
    private readonly ResponsibleEntityService responsibleService;
    private readonly IOccurrenceService occurrenceService;
    private readonly NotifierClient notifierClient;
    private readonly ReportServiceImpl service;
    private readonly CountingHttpHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestReportService_Integration"/> class.
    /// </summary>
    /// <param name="fixture">the database fixture</param>
    public TestReportService_Integration(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        context = fixture.Context;

        reportRepo = new ReportRepository(context);
        occurrenceRepo = new OccurrenceRepository(context);
        responsibleService = new ResponsibleEntityService(context);
        occurrenceService = new OccurrenceServiceImpl(occurrenceRepo, responsibleService);

        handler = new CountingHttpHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        notifierClient = new NotifierClient(http, NullLogger<NotifierClient>.Instance);

        service = new ReportServiceImpl(
            reportRepo,
            occurrenceService,
            responsibleService,
            notifierClient
        );
    }

    /// <summary>
    /// Tests that creating a report persists it and creates an occurrence.
    /// </summary>
    [Fact]
    public void CreateReport_PersistsReportAndCreatesOccurrence()
    {
        fixture.ResetDatabase();

        var user = UserFixture.CreateOrUpdateUser(email: $"cavaco-{Guid.NewGuid():N}@example.com");
        context.Users.Add(user);
        context.SaveChanges();

        var dto = new CreateReportDto
        {
            Title = "Emergência Médica",
            Description = "Atropelamento",
            Type = OccurrenceType.TRAFFIC_CONGESTION,
            UserId = user.Id,
            Latitude = 41.366943,
            Longitude = -8.194949,
        };

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var lon = dto.Longitude;
        var lat = dto.Latitude;
        var coords = new[]
        {
            new Coordinate(lon - 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat - 0.01),
        };
        var poly = geometryFactory.CreatePolygon(coords);
        var multi = geometryFactory.CreateMultiPolygon(new[] { poly });

        var entity = new ResponsibleEntity
        {
            Name = "Bombeiros Lisboa",
            Email = "bombeiros@lisboa.pt",
            Address = "Rua X, Lisboa",
            ContactPhone = 213456789,
            Type = ResponsibleEntityType.POLICIA,
            GeoArea = multi,
        };

        context.ResponsibleEntities.Add(entity);
        context.SaveChanges();

        var report = ReportFixture.CreateOrUpdate(
            title: dto.Title,
            description: dto.Description,
            userId: dto.UserId,
            type: dto.Type,
            location: new GeoPoint { Latitude = dto.Latitude, Longitude = dto.Longitude }
        );

        var (createdReport, _) = service.Create(report);

        var dbReport = context.Reports.FirstOrDefault(r => r.Id == createdReport.Id);
        Assert.NotNull(dbReport);
        Assert.Equal(dto.Title, dbReport!.Title);
        Assert.Equal(user.Id, dbReport.UserId);

        var dbOcc = context.Occurrences.FirstOrDefault(o => o.ReportId == createdReport.Id);
        Assert.NotNull(dbOcc);
        Assert.Equal(OccurrenceStatus.WAITING, dbOcc!.Status);
        Assert.Equal(1, dbOcc.ReportCount);

        if (dbOcc.ResponsibleEntityId > 0)
            Assert.Equal(entity.Id, dbOcc.ResponsibleEntityId);

        Assert.Equal(0, handler.Calls);
    }

    /// <summary>
    /// Tests that creating 3 duplicate reports changes the occurrence' status to active and notifies the responsible entity.
    /// </summary>
    [Fact]
    public void CreateDuplicate_ReachesTrigger_ActivatesOccurrenceAndNotifies()
    {
        fixture.ResetDatabase();

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var lon = -8.194949;
        var lat = 41.366943;
        var coords = new[]
        {
            new Coordinate(lon - 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat - 0.01),
        };
        var poly = geometryFactory.CreatePolygon(coords);
        var multi = geometryFactory.CreateMultiPolygon(new[] { poly });

        var entity = new ResponsibleEntity
        {
            Name = "Bombeiros Lisboa",
            Email = "bombeiros@lisboa.pt",
            Address = "Rua X, Lisboa",
            ContactPhone = 213456789,
            Type = ResponsibleEntityType.POLICIA,
            GeoArea = multi,
        };
        context.ResponsibleEntities.Add(entity);
        context.SaveChanges();

        var user1 = UserFixture.CreateOrUpdateUser(email: $"user1-{Guid.NewGuid():N}@example.com");
        var user2 = UserFixture.CreateOrUpdateUser(email: $"user2-{Guid.NewGuid():N}@example.com");
        var user3 = UserFixture.CreateOrUpdateUser(email: $"user3-{Guid.NewGuid():N}@example.com");
        context.Users.AddRange(user1, user2, user3);
        context.SaveChanges();

        var report1 = ReportFixture.CreateOrUpdate(
            title: "dup-1",
            description: "dup",
            userId: user1.Id,
            type: OccurrenceType.CRIME,
            location: new GeoPoint { Latitude = lat, Longitude = lon }
        );

        var report2 = ReportFixture.CreateOrUpdate(
            title: "dup-2",
            description: "dup",
            userId: user2.Id,
            type: OccurrenceType.CRIME,
            location: new GeoPoint { Latitude = lat, Longitude = lon }
        );

        var report3 = ReportFixture.CreateOrUpdate(
            title: "dup-3",
            description: "dup",
            userId: user3.Id,
            type: OccurrenceType.CRIME,
            location: new GeoPoint { Latitude = lat, Longitude = lon }
        );

        var (_, occ1) = service.Create(report1);
        Assert.NotNull(occ1);
        Assert.Equal(1, occ1.ReportCount);
        Assert.Equal(OccurrenceStatus.WAITING, occ1.Status);

        context
            .ChangeTracker.Entries<Occurrence>()
            .ToList()
            .ForEach(e => e.State = EntityState.Detached);

        var (_, occ2) = service.Create(report2);
        Assert.NotNull(occ2);
        Assert.Equal(2, occ2.ReportCount);
        Assert.Equal(OccurrenceStatus.WAITING, occ2.Status);

        context
            .ChangeTracker.Entries<Occurrence>()
            .ToList()
            .ForEach(e => e.State = EntityState.Detached);

        var (_, occ3) = service.Create(report3);
        Assert.NotNull(occ3);
        Assert.Equal(3, occ3.ReportCount);
        Assert.Equal(OccurrenceStatus.ACTIVE, occ3.Status);

        context
            .ChangeTracker.Entries<Occurrence>()
            .ToList()
            .ForEach(e => e.State = EntityState.Detached);

        var dbOcc = context.Occurrences.FirstOrDefault(o => o.Id == occ3.Id);
        Assert.NotNull(dbOcc);
        Assert.Equal(3, dbOcc!.ReportCount);
        Assert.Equal(OccurrenceStatus.ACTIVE, dbOcc.Status);

        var called = SpinWait.SpinUntil(() => handler.Calls > 0, TimeSpan.FromSeconds(5));
        Assert.True(called, "Notifier was not called within timeout");
        Assert.Equal(1, handler.Calls);
    }

    /// <summary>
    /// A HttpMessageHandler that counts the number of calls made.
    /// </summary>
    private class CountingHttpHandler : HttpMessageHandler
    {
        private int calls;

        public int Calls => calls;

        /// <summary>
        /// Sends an HTTP request and increments the call count.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Interlocked.Increment(ref calls);
            var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"ok\":true}",
                    System.Text.Encoding.UTF8,
                    "application/json"
                ),
            };
            return Task.FromResult(resp);
        }
    }
}

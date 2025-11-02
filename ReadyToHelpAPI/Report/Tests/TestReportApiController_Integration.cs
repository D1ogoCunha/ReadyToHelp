namespace readytohelpapi.Report.Tests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Notifications;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Report.Controllers;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Report.Models;
using readytohelpapi.Report.Services;
using readytohelpapi.ResponsibleEntity.Models;
using readytohelpapi.ResponsibleEntity.Services;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

/// <summary>
/// This class contains all integration tests for ReportApiController.
/// </summary>
[Trait("Category", "Integration")]
public class TestReportApiController_Integration : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext context;
    private readonly ReportApiController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestReportApiController_Integration"/> class.
    /// </summary>
    public TestReportApiController_Integration(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        this.context = this.fixture.Context;

        var reportRepo = new ReportRepository(context);
        var occurrenceRepo = new OccurrenceRepository(context);
        var responsibleService = new ResponsibleEntityService(context);
        var occurrenceService = new OccurrenceServiceImpl(occurrenceRepo, responsibleService);

        var http = new HttpClient(new FakeHttpHandler())
        {
            BaseAddress = new Uri("http://localhost"),
        };
        var notifierClient = new NotifierClient(http, NullLogger<NotifierClient>.Instance);

        var reportService = new ReportServiceImpl(
            reportRepo,
            occurrenceService,
            responsibleService,
            notifierClient
        );

        controller = new ReportApiController(reportService, reportRepo, context);
        controller.ModelState.Clear();
    }

    /// <summary>
    /// Tests the Create method with a responsible entity present in DB.
    /// </summary>
    [Fact]
    public void Create_WithResponsibleEntity_ReturnsEntityInResponse()
    {
        fixture.ResetDatabase();

        var user = UserFixture.CreateOrUpdateUser(email: $"cavaco-{Guid.NewGuid():N}@example.com");
        context.Users.Add(user);
        context.SaveChanges();
        var userId = user.Id;

        var dto = new CreateReportDto
        {
            Title = "Emergência Médica",
            Description = "Atropelamento",
            Type = OccurrenceType.TRAFFIC_CONGESTION,
            UserId = userId,
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

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.True(created.RouteValues?.ContainsKey("id"));

        var savedReport = context.Reports.FirstOrDefault(r =>
            r.Title == dto.Title && r.UserId == dto.UserId
        );
        Assert.NotNull(savedReport);

        var savedOccurrence = context.Occurrences.FirstOrDefault(o =>
            o.ReportId == savedReport!.Id
        );
        Assert.NotNull(savedOccurrence);

        Assert.Equal(entity.Id, savedOccurrence!.ResponsibleEntityId);
        var response = Assert.IsType<ReportResponseDto>(created.Value);
        Assert.Equal(savedReport!.Id, response.ReportId);
        Assert.Equal(savedOccurrence.Id, response.OccurrenceId);
    }

    /// <summary>
    /// Tests the GetById method when the report is found.
    /// </summary>
    [Fact]
    public void GetById_ReturnsOk_WhenReportExists()
    {
        fixture.ResetDatabase();

        var user = UserFixture.CreateOrUpdateUser(email: $"test-{Guid.NewGuid():N}@example.com");
        context.Users.Add(user);
        context.SaveChanges();
        var userId = user.Id;

        var report = new Report
        {
            Title = "Relatório integração",
            Description = "descrição",
            UserId = userId,
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.0, Longitude = -8.0 },
        };

        context.Reports.Add(report);
        context.SaveChanges();

        var id = report.Id;

        var result = controller.GetById(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<Report>(ok.Value);
        Assert.Equal(id, payload.Id);
        Assert.Equal("Relatório integração", payload.Title);
    }

    /// <summary>
    /// Tests the GetById method when the report is not found.
    /// </summary>
    [Fact]
    public void GetById_NotFound_ReturnsNotFound()
    {
        fixture.ResetDatabase();
        var result = controller.GetById(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    /// Fake HTTP handler for testing.
    /// </summary>
    private class FakeHttpHandler : HttpMessageHandler
    {
        /// <summary>
        /// Sends an HTTP request asynchronously.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The HTTP response message.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
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

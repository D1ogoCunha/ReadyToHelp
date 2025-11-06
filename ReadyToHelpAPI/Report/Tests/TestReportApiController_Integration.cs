namespace readytohelpapi.Report.Tests;

using System.Net;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Common.Tests;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Report.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

/// <summary>
/// This class contains all integration tests for ReportApiController.
/// </summary>
[Trait("Category", "Integration")]
public class TestReportApiController_Integration : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory factory;
    private readonly HttpClient client;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TestReportApiController_Integration(TestWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    private static async Task WithDbAsync(TestWebApplicationFactory factory, Func<AppDbContext, Task> action)
    {
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(ctx);
    }

    [Fact]
    public async Task Create_WithResponsibleEntity_ReturnsEntityInResponse()
    {
        int userId = 0;
        await WithDbAsync(factory, async ctx =>
        {
            await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""reports"" RESTART IDENTITY CASCADE;");
            await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""occurrences"" RESTART IDENTITY CASCADE;");
            await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""responsible_entities"" RESTART IDENTITY CASCADE;");

            var user = readytohelpapi.User.Tests.Fixtures.UserFixture.CreateOrUpdateUser(
                email: $"int-{Guid.NewGuid():N}@example.com"
            );
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;

            var lat = 41.366943;
            var lon = -8.194949;
            var gf = new GeometryFactory(new PrecisionModel(), 4326);
            var poly = gf.CreatePolygon(new[]
            {
                new Coordinate(lon-0.01, lat-0.01),
                new Coordinate(lon-0.01, lat+0.01),
                new Coordinate(lon+0.01, lat+0.01),
                new Coordinate(lon+0.01, lat-0.01),
                new Coordinate(lon-0.01, lat-0.01),
            });
            var multi = gf.CreateMultiPolygon(new[] { poly });

            ctx.ResponsibleEntities.Add(new readytohelpapi.ResponsibleEntity.Models.ResponsibleEntity
            {
                Name = "Bombeiros",
                Email = "b@x.pt",
                Address = "R1",
                ContactPhone = 999999999,
                Type = readytohelpapi.ResponsibleEntity.Models.ResponsibleEntityType.POLICIA,
                GeoArea = multi
            });
            await ctx.SaveChangesAsync();
        });

        var dto = new CreateReportDto
        {
            Title = "Emergência Médica",
            Description = "Atropelamento",
            Type = OccurrenceType.TRAFFIC_CONGESTION,
            UserId = userId,
            Latitude = 41.366943,
            Longitude = -8.194949
        };

        var resp = await client.PostAsJsonAsync("/api/reports", dto);
        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<ReportResponseDto>(Json);
        Assert.NotNull(payload);
        Assert.True(payload!.ReportId > 0);
        Assert.True(payload.OccurrenceId > 0);
        Assert.NotNull(payload.ResponsibleEntity);
    }
}
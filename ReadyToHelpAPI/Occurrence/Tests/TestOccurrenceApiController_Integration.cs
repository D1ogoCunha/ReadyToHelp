namespace readytohelpapi.Occurrence.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using readytohelpapi.Common.Tests;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using Xunit;

public partial class Program { }

[Trait("Category", "Integration")]
public class TestOccurrenceApiController : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<DbFixture>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DbFixture _dbFixture;
    private readonly HttpClient _client;

    public TestOccurrenceApiController(WebApplicationFactory<Program> factory, DbFixture dbFixture)
    {
        _dbFixture = dbFixture;

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test",
                        _ => { });

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task ClearDataAsync()
    {
        var ctx = _dbFixture.Context;
        ctx.ChangeTracker.Clear();

        await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""occurrences"" RESTART IDENTITY CASCADE;");
        await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""responsible_entities"" RESTART IDENTITY CASCADE;");
        await ctx.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""reports"" RESTART IDENTITY CASCADE;");

        ctx.ChangeTracker.Clear();
    }

    private async Task<ResponsibleEntity> SeedResponsibleEntityAsync(string name = "Seed RE")
    {
        var ctx = _dbFixture.Context;
        ctx.ChangeTracker.Clear();

        var geomFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var coords = new[]
        {
            new Coordinate(-9.158, 38.715),
            new Coordinate(-9.158, 38.725),
            new Coordinate(-9.14,  38.725),
            new Coordinate(-9.14,  38.715),
            new Coordinate(-9.158, 38.715)
        };
        var poly = geomFactory.CreatePolygon(coords);

        var re = new ResponsibleEntity
        {
            Name = name,
            Email = $"{Guid.NewGuid():N}@local",
            Address = "Seed St",
            ContactPhone = 900000000,
            Type = 0,
            GeoArea = geomFactory.CreateMultiPolygon(new[] { poly })
        };

        ctx.ResponsibleEntities.Add(re);
        await ctx.SaveChangesAsync();
        ctx.Entry(re).State = EntityState.Detached;
        return re;
    }

    private async Task<Occurrence> SeedOccurrenceAsync(
        string title,
        OccurrenceStatus status = OccurrenceStatus.ACTIVE,
        double lat = 40.0,
        double lon = -8.0,
        int? responsibleEntityId = null)
    {
        var ctx = _dbFixture.Context;
        ctx.ChangeTracker.Clear();

        var occ = new Occurrence
        {
            Title = title,
            Description = "seed",
            Type = OccurrenceType.FLOOD,
            Status = status,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 10,
            ReportId = null,
            ResponsibleEntityId = responsibleEntityId,
            Location = new GeoPoint { Latitude = lat, Longitude = lon }
        };

        ctx.Occurrences.Add(occ);
        await ctx.SaveChangesAsync();
        ctx.Entry(occ).State = EntityState.Detached;
        return occ;
    }

    [Fact]
    public async Task Create_ReturnsCreated_AndBodyHasId()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var body = new
        {
            title = "Flood near river",
            description = "High water level",
            type = 1,
            status = 1,
            priority = 1,
            proximityRadius = 150,
            location = new { latitude = 38.72, longitude = -9.15 },
            responsibleEntityId = re.Id
        };

        var resp = await _client.PostAsync(
            "/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
        json.Should().ContainKey("id");
        json["title"]!.ToString().Should().Be("Flood near river");
    }

    [Fact]
    public async Task Update_ReturnsOk_AndPersists()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var create = new
        {
            title = "Old Title",
            description = "seed",
            type = 1,
            status = 1,
            priority = 0,
            proximityRadius = 10,
            location = new { latitude = 40.0, longitude = -8.0 },
            responsibleEntityId = re.Id
        };
        var created = await _client.PostAsync("/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(create), Encoding.UTF8, "application/json"));
        created.EnsureSuccessStatusCode();
        var id = JObject.Parse(await created.Content.ReadAsStringAsync())["id"]!.Value<int>();

        var update = new
        {
            id,
            title = "Updated Title",
            description = "Updated",
            type = 1,
            status = 2,
            priority = 2,
            location = new { latitude = 40.2, longitude = -8.6 },
            responsibleEntityId = re.Id
        };

        var resp = await _client.PutAsync(
            "/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JObject.Parse(await resp.Content.ReadAsStringAsync());
        body["title"]!.ToString().Should().Be("Updated Title");
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsBadRequest()
    {
        var resp = await _client.DeleteAsync("/api/occurrence/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        await ClearDataAsync();
        var resp = await _client.GetAsync("/api/occurrence/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithFilterAndSort_ReturnsOk()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();
        await SeedOccurrenceAsync("apple", responsibleEntityId: re.Id);
        await SeedOccurrenceAsync("banana", responsibleEntityId: re.Id);
        await SeedOccurrenceAsync("cantaloupe", responsibleEntityId: re.Id);

        var resp = await _client.GetAsync("/api/occurrence?pageNumber=1&pageSize=10&sortBy=title&sortOrder=asc&filter=an");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_InvalidPaging_ReturnsBadRequest()
    {
        var resp = await _client.GetAsync("/api/occurrence?pageNumber=-1&pageSize=-5");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllActive_NoData_ReturnsNotFound()
    {
        await ClearDataAsync();

        var resp = await _client.GetAsync("/api/occurrence/active");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            JArray.Parse(await resp.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task GetActive_FilterByType_ReturnsOnlyMatching()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        await SeedOccurrenceAsync("Fire1", status: OccurrenceStatus.ACTIVE, responsibleEntityId: re.Id);
        await SeedOccurrenceAsync("Flood1", status: OccurrenceStatus.ACTIVE, responsibleEntityId: re.Id);

        var resp = await _client.GetAsync("/api/occurrence/active?type=FLOOD");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JArray.Parse(await resp.Content.ReadAsStringAsync());
        json.Should().OnlyContain(x => x["type"]!.ToString() == "FLOOD");
    }

    [Fact]
    public async Task Update_NullBody_ReturnsBadRequest()
    {
        var resp = await _client.PutAsync("/api/occurrence",
            new StringContent("", Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

     [Fact]
    public async Task Create_NullBody_ReturnsBadRequest()
    {
        var resp = await _client.PostAsync(
            "/api/occurrence",
            new StringContent("", Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_SetsLocationHeader_WithCreatedId()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var body = new
        {
            title = "Created With Location",
            description = "desc",
            type = 1,
            status = 1,
            priority = 1,
            proximityRadius = 50,
            location = new { latitude = 38.72, longitude = -9.15 },
            responsibleEntityId = re.Id
        };

        var resp = await _client.PostAsync(
            "/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location.Should().NotBeNull();
        resp.Headers.Location!.ToString().Should().Contain("/api/occurrence/");
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk_AndDtoIsMapped()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var create = new
        {
            title = "Dto Mapping",
            description = "d",
            type = 1,
            status = 1,
            priority = 0,
            proximityRadius = 10,
            location = new { latitude = 40.1, longitude = -8.1 },
            responsibleEntityId = re.Id
        };
        var created = await _client.PostAsync("/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(create), Encoding.UTF8, "application/json"));
        created.EnsureSuccessStatusCode();
        var id = JObject.Parse(await created.Content.ReadAsStringAsync())["id"]!.Value<int>();

        var resp = await _client.GetAsync($"/api/occurrence/{id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = JObject.Parse(await resp.Content.ReadAsStringAsync());
        dto["id"]!.Value<int>().Should().Be(id);
        dto["title"]!.ToString().Should().Be("Dto Mapping");
        dto["latitude"]!.Value<double>().Should().BeApproximately(40.1, 1e-6);
        dto["longitude"]!.Value<double>().Should().BeApproximately(-8.1, 1e-6);
        dto["endDateTime"]!.Type.Should().Be(Newtonsoft.Json.Linq.JTokenType.Null); // controller maps default to null
    }

    [Fact]
    public async Task Update_NotExisting_ReturnsNotFoundOr500_FromService()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var update = new
        {
            id = 999999,
            title = "Does Not Exist",
            description = "x",
            type = 1,
            status = 2,
            priority = 2,
            location = new { latitude = 40.0, longitude = -8.0 },
            responsibleEntityId = re.Id
        };

        var resp = await _client.PutAsync(
            "/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsOk_AndThenNotFoundOnGet()
    {
        await ClearDataAsync();
        var re = await SeedResponsibleEntityAsync();

        var create = new
        {
            title = "To Delete",
            description = "x",
            type = 1,
            status = 1,
            priority = 0,
            proximityRadius = 10,
            location = new { latitude = 39.9, longitude = -8.9 },
            responsibleEntityId = re.Id
        };
        var created = await _client.PostAsync("/api/occurrence",
            new StringContent(JsonConvert.SerializeObject(create), Encoding.UTF8, "application/json"));
        created.EnsureSuccessStatusCode();
        var id = JObject.Parse(await created.Content.ReadAsStringAsync())["id"]!.Value<int>();

        var del = await _client.DeleteAsync($"/api/occurrence/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await _client.GetAsync($"/api/occurrence/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        await ClearDataAsync();
        var resp = await _client.DeleteAsync("/api/occurrence/987654");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllActive_FilterByPriority_AndResponsibleEntity_ReturnsOnlyMatching()
    {
        await ClearDataAsync();
        var re1 = await SeedResponsibleEntityAsync("R1");
        var re2 = await SeedResponsibleEntityAsync("R2");


        await SeedOccurrenceAsync("O1", status: OccurrenceStatus.ACTIVE, responsibleEntityId: re1.Id);

        var ctx = _dbFixture.Context;
        ctx.Occurrences.Add(new Occurrence
        {
            Title = "O2",
            Description = "x",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.HIGH,
            Location = new GeoPoint { Latitude = 38.71, Longitude = -9.14 },
            ResponsibleEntityId = re1.Id
        });

        ctx.Occurrences.Add(new Occurrence
        {
            Title = "O3",
            Description = "x",
            Type = OccurrenceType.FOREST_FIRE,
            Status = OccurrenceStatus.ACTIVE,
            Priority = PriorityLevel.HIGH,
            Location = new GeoPoint { Latitude = 38.72, Longitude = -9.15 },
            ResponsibleEntityId = re2.Id
        });
        await ctx.SaveChangesAsync();

        var resp = await _client.GetAsync($"/api/occurrence/active?priority=HIGH&responsibleEntityId={re1.Id}&pageSize=10&pageNumber=1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var arr = JArray.Parse(await resp.Content.ReadAsStringAsync());
        arr.Should().NotBeEmpty();
        arr.Should().OnlyContain(t => t["priority"]!.ToString() == "HIGH");
    }
}
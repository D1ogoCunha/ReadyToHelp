namespace readytohelpapi.Occurrence.Tests;

using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using readytohelpapi.Common.Data;
using readytohelpapi.Common.Tests;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;
using Xunit;

/// <summary>
/// This class contains all integration tests for the Occurrence API controller.
/// </summary>
[Trait("Category", "Integration")]
public class TestOccurrenceApiController_Integration
    : IClassFixture<TestWebApplicationFactory>,
        IClassFixture<DbFixture>
{
    private readonly HttpClient _client;
    private readonly DbFixture fixture;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestOccurrenceApiController_Integration"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    /// <param name="dbFixture">The database fixture.</param>
    public TestOccurrenceApiController_Integration(
        TestWebApplicationFactory factory,
        DbFixture dbFixture
    )
    {
        fixture = dbFixture;

        fixture.ResetDatabase();

        var connection = fixture.Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var customized = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(connection, npgsql => npgsql.UseNetTopologySuite());
                });
            });
        });

        _client = customized.CreateClient();
    }

    /// <summary>
    /// Tests the GetAll occurrences endpoint.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsOk_AndList()
    {
        var response = await _client.GetAsync("/api/occurrence");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());

        var list = await response.Content.ReadFromJsonAsync<List<Occurrence>>(options);
        Assert.NotNull(list);
    }

    /// <summary>
    /// Tests the GetById occurrences endpoint when the occurrence is missing.
    /// </summary>
    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissing()
    {
        var response = await _client.GetAsync("/api/occurrence/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests the GetById occurrences endpoint when the occurrence ID is invalid.
    /// </summary>
    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        var resp = await _client.GetAsync("/api/occurrence/0");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    /// <summary>
    /// Tests the GetAllActive occurrences endpoint.
    /// </summary>
    [Fact]
    public async Task GetAllActive_ReturnsOkOrNotFound()
    {
        var response = await _client.GetAsync("/api/occurrence/active");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.NotFound
        );
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var list = await response.Content.ReadFromJsonAsync<List<object>>();
            Assert.NotNull(list);
        }
    }

    /// <summary>
    /// Asserts that the response is created (201) or throws an exception.
    /// </summary>
    private static async Task AssertCreatedOrThrow(HttpResponseMessage resp)
    {
        if (resp.StatusCode != HttpStatusCode.Created)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Expected 201 Created but got {(int)resp.StatusCode} {resp.StatusCode}. Response body:{Environment.NewLine}{body}"
            );
        }
    }

    /// <summary>
    /// Tests the Create occurrence endpoint with a valid request.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var createDto = new Occurrence
        {
            Title = "integration create",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.340044, Longitude = -8.0 },
        };

        var response = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        await AssertCreatedOrThrow(response);

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var createdDto = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(location, options);
        Assert.NotNull(createdDto);
        Assert.Equal("integration create", createdDto.Title);
    }

    /// <summary>
    /// Tests the Create occurrence endpoint when the title is missing.
    /// </summary>
    [Fact]
    public async Task Create_MissingTitle_ReturnsBadRequest()
    {
        var createDto = new Occurrence
        {
            Title = "",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.340044, Longitude = -8.0 },
        };

        var response = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Tests the Delete occurrence endpoint.
    /// </summary>
    [Fact]
    public async Task Delete_AndReturnsOk()
    {
        var createDto = new OccurrenceCreateDto
        {
            Title = "to delete",
            Description = "desc delete",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.LOW,
            ProximityRadius = 0,
            ReportCount = 0,
            ReportId = null,
            ResponsibleEntityId = 0,
            Location = new GeoPoint { Latitude = 40.0, Longitude = -8.0 },
        };

        var createResp = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        await AssertCreatedOrThrow(createResp);

        var location = createResp.Headers.Location;
        Assert.NotNull(location);

        var segments = location!.AbsolutePath.TrimEnd('/').Split('/');
        var idStr = segments[^1];
        Assert.True(int.TryParse(idStr, out var id));

        var del = await _client.DeleteAsync($"/api/occurrence/{id}");
        Assert.Equal(HttpStatusCode.OK, del.StatusCode);

        var get = await _client.GetAsync($"/api/occurrences/{id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    /// <summary>
    /// Tests the Delete occurrence endpoint when the ID is invalid.
    /// </summary>
    [Fact]
    public async Task Delete_InvalidId_ReturnsBadRequest()
    {
        var resp = await _client.DeleteAsync("/api/occurrence/0");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    /// <summary>
    /// Tests the Delete occurrence endpoint when the occurrence does not exist.
    /// </summary>
    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        var resp = await _client.DeleteAsync("/api/occurrence/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Tests the Update occurrence endpoint with a valid request.
    /// </summary>
    [Fact]
    public async Task Update_Succeeds_ReturnsOk_AndPersisted()
    {
        var createDto = new Occurrence
        {
            Title = "to update",
            Description = "original",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.0, Longitude = -8.0 },
        };

        var createResp = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        await AssertCreatedOrThrow(createResp);

        var location = createResp.Headers.Location;
        Assert.NotNull(location);
        var segments = location!.AbsolutePath.TrimEnd('/').Split('/');
        var idStr = segments[^1];
        Assert.True(int.TryParse(idStr, out var id));

        var updated = new Occurrence
        {
            Id = id,
            Title = "updated title",
            Description = "updated desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 42.0, Longitude = -7.0 },
        };

        var put = await _client.PutAsJsonAsync("/api/occurrence", updated);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var dto = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(
            $"/api/occurrence/{id}",
            options
        );
        Assert.NotNull(dto);
        Assert.Equal("updated title", dto!.Title);
        Assert.Equal("updated desc", dto.Description);
        Assert.InRange(dto.Latitude, -90.0, 90.0);
        Assert.InRange(dto.Longitude, -180.0, 180.0);
    }

    /// <summary>
    /// Tests the Update occurrence endpoint when the ID is invalid.
    /// </summary>
    [Fact]
    public async Task Update_InvalidId_ReturnsBadRequest()
    {
        var payload = new Occurrence
        {
            Id = 0,
            Title = "x",
            Description = "x",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.0, Longitude = -8.0 },
        };

        var resp = await _client.PutAsJsonAsync("/api/occurrence", payload);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    /// <summary>
    /// Tests the GetAllActive occurrences endpoint .
    /// </summary>
    [Fact]
    public async Task GetAllActive_ReturnsMappedOccurrenceMapDto()
    {
        var createDto = new Occurrence
        {
            Title = "map test",
            Description = "map desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 38.7, Longitude = -9.1 },
        };

        var createResp = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        await AssertCreatedOrThrow(createResp);

        var location = createResp.Headers.Location;
        Assert.NotNull(location);

        var segments = location!.AbsolutePath.TrimEnd('/').Split('/');
        var idStr = segments[^1];
        Assert.True(int.TryParse(idStr, out var createdId));

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());

        var createdDto = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(location, options);
        Assert.NotNull(createdDto);

        var activatePayload = new Occurrence
        {
            Id = createdId,
            Title = createdDto.Title,
            Description = createdDto.Description,
            Type = createdDto.Type,
            Location = new GeoPoint
            {
                Latitude = createdDto.Latitude,
                Longitude = createdDto.Longitude,
            },
            Status = OccurrenceStatus.ACTIVE,
        };

        var putResp = await _client.PutAsJsonAsync("/api/occurrence", activatePayload);
        Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);

        var resp = await _client.GetAsync("/api/occurrence/active?pageSize=100");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var list = await resp.Content.ReadFromJsonAsync<List<OccurrenceMapDto>>(options);
        Assert.NotNull(list);

        var mapped = list!.FirstOrDefault(m => m.Id == createdId);
        Assert.NotNull(mapped);
        Assert.Equal(createDto.Title, mapped!.Title);
        Assert.Equal(createDto.Type, mapped.Type);
        Assert.InRange(mapped.Latitude, -90.0, 90.0);
        Assert.InRange(mapped.Longitude, -180.0, 180.0);
        Assert.Equal(createDto.Location.Latitude, mapped.Latitude);
        Assert.Equal(createDto.Location.Longitude, mapped.Longitude);
    }
}

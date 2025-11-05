namespace readytohelpapi.Occurrence.Tests;

using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Tests;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;
using Xunit;

public partial class Program { }

/// <summary>
/// This class contains all integration tests for the Occurrence API controller.
/// </summary>
[Trait("Category", "Integration")]
public class TestOccurrenceApiController : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestOccurrenceApiController"/> class.
    /// </summary>
    /// <param name="factory">The web application factory used to create the test server and client.</param>
    public TestOccurrenceApiController(WebApplicationFactory<Program> factory)
    {
        var customized = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test",
                        options => { }
                    );

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        });

        _client = customized.CreateClient();
    }

    /// <summary>
    /// Tests the GetAll endpoint of the Occurrence API.
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
    /// Tests the GetById endpoint of the Occurrence API.
    /// </summary>
    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissing()
    {
        var response = await _client.GetAsync("/api/occurrences/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests the GetAllActive endpoint of the Occurrence API.
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
    /// Tests the Create endpoint of the Occurrence API.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var occ = new Occurrence
        {
            Title = "integration create",
            Description = "desc",
            Type = OccurrenceType.ROAD_DAMAGE,
            Priority = PriorityLevel.MEDIUM,
            Location = new GeoPoint { Latitude = 40.0, Longitude = -8.0 },
        };

        var response = await _client.PostAsJsonAsync("/api/occurrence", occ);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var createdDto = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(location, options);
        Assert.NotNull(createdDto);
        Assert.Equal("integration create", createdDto.Title);
    }

    /// <summary>
    /// Tests the Update endpoint of the Occurrence API.
    /// </summary>
    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var payload = new Occurrence
        {
            Title = "to update",
            Description = "desc update",
            Type = OccurrenceType.FLOOD,
            Location = new GeoPoint { Latitude = 40.0, Longitude = -8.0 },
        };
        var createResp = await _client.PostAsJsonAsync("/api/occurrence", payload);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var location = createResp.Headers.Location;
        Assert.NotNull(location);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());

        var createdDto = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(location, options);
        Assert.NotNull(createdDto);

        var updatePayload = new Occurrence
        {
            Id = createdDto.Id,
            Title = "updated title",
            Description = createdDto.Description ?? "desc update",
            Type = createdDto.Type,
            Location = new GeoPoint
            {
                Latitude = createdDto.Latitude,
                Longitude = createdDto.Longitude,
            },
            CreationDateTime = createdDto.CreationDateTime,
            EndDateTime = createdDto.EndDateTime ?? default,
            ReportCount = createdDto.ReportCount,
            ResponsibleEntityId = createdDto.ResponsibleEntityId ?? 0,
            Priority = createdDto.Priority,
        };

        var put = await _client.PutAsJsonAsync("/api/occurrence", updatePayload);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var fetched = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(
            $"/api/occurrences/{createdDto.Id}",
            options
        );
        Assert.NotNull(fetched);
        Assert.Equal("updated title", fetched.Title);
    }

    /// <summary>
    /// Tests the Delete endpoint of the Occurrence API.
    /// </summary>
    [Fact]
    public async Task Delete_RemovesResource_AndReturnsOk()
    {
        var occ = new Occurrence
        {
            Title = "to delete",
            Description = "desc delete",
            Type = OccurrenceType.FLOOD,
            Location = new GeoPoint { Latitude = 40.0, Longitude = -8.0 },
        };
        var createResp = await _client.PostAsJsonAsync("/api/occurrence", occ);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

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
}

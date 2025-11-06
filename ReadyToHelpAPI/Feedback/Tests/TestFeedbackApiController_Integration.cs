namespace readytohelpapi.Feedback.Tests;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using readytohelpapi.Common.Data;
using readytohelpapi.Common.Tests;
using readytohelpapi.User.Models;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Tests.Fixtures;
using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;
using Xunit;

public partial class Program { }

/// <summary>
///   This class contains all integration tests for the Feedback API controller.
/// </summary>
[Trait("Category", "Integration")]
public class TestFeedbackApiController_Integration
    : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<DbFixture>
{
    private readonly HttpClient _client;
    private readonly DbFixture _fixture;
    private readonly JsonSerializerOptions _json;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFeedbackApiController_Integration"/> class.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="dbFixture"></param>
    public TestFeedbackApiController_Integration(
        WebApplicationFactory<Program> factory,
        DbFixture dbFixture
    )
    {
        _fixture = dbFixture;
        _fixture.ResetDatabase();

        var conn = _fixture.Context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            conn.Open();

        var customized = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));

                services.AddDbContext<AppDbContext>(opts =>
                {
                    opts.UseNpgsql(conn, npgsql => npgsql.UseNetTopologySuite());
                });

                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        });

        using (var scope = customized.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (int uid = 1; uid <= 5; uid++)
            {
                var existing = ctx.Users.Find(uid);
                if (existing == null)
                {
                    ctx.Users.Add(new User
                    {
                        Id = uid,
                        Name = $"Test User {uid}",
                        Email = $"test{uid}@local",
                        Password = "pwd",
                        Profile = Profile.CITIZEN
                    });
                }
            }
            ctx.SaveChanges();
        }

        _client = customized.CreateClient();

        _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _json.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Asserts that the response has status code 201 Created, otherwise throws an exception.
    /// </summary>
    /// <param name="resp"></param>
    private static async Task AssertCreatedOrThrow(HttpResponseMessage resp)
    {
        if (resp.StatusCode != HttpStatusCode.Created)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Expected 201 Created but got {(int)resp.StatusCode} {resp.StatusCode}. Body:{Environment.NewLine}{body}"
            );
        }
    }

    /// <summary>
    /// Parses the identifier from a Location URI.
    /// </summary>
    /// <param name="location"></param>
    /// <returns> The parsed identifier.</returns>
    private static int ParseIdFromLocation(Uri location)
    {
        var segments = location.AbsolutePath.TrimEnd('/').Split('/');
        var idStr = segments[^1];
        Assert.True(int.TryParse(idStr, out var id));
        return id;
    }

    /// <summary>
    /// Creates an occurrence, optionally activating it.
    /// </summary>
    /// <param name="activate"> Whether to activate the occurrence after creation. </param>
    private async Task<(int id, OccurrenceDetailsDto details)> CreateOccurrenceAsync(bool activate)
    {
        var createDto = new Occurrence
        {
            Title = "fb occ",
            Description = "for feedback",
            Type = OccurrenceType.ROAD_DAMAGE,
            Location = new GeoPoint { Latitude = 41.15, Longitude = -8.61 },
        };

        var createResp = await _client.PostAsJsonAsync("/api/occurrence", createDto);
        await AssertCreatedOrThrow(createResp);
        Assert.NotNull(createResp.Headers.Location);
        var id = ParseIdFromLocation(createResp.Headers.Location!);

        var details = await _client.GetFromJsonAsync<OccurrenceDetailsDto>(createResp.Headers.Location!, _json);
        Assert.NotNull(details);

        if (activate)
        {
            var activatePayload = new Occurrence
            {
                Id = id,
                Title = details!.Title,
                Description = details.Description,
                Type = details.Type,
                Location = new GeoPoint { Latitude = details.Latitude, Longitude = details.Longitude },
                Status = OccurrenceStatus.ACTIVE,
            };

            var putResp = await _client.PutAsJsonAsync("/api/occurrence", activatePayload);
            Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);

            details = await _client.GetFromJsonAsync<OccurrenceDetailsDto>($"/api/occurrences/{id}", _json);
            Assert.NotNull(details);
            Assert.Equal(OccurrenceStatus.ACTIVE, details!.Status);
        }

        return (id, details!);
    }

    /// <summary>
    /// Posts feedback for the given occurrence and user.
    /// </summary>
    /// <param name="occurrenceId"></param>
    /// <param name="userId"></param>
    /// <param name="isConfirmed"></param>
    private Task<HttpResponseMessage> PostFeedbackAsync(int occurrenceId, int userId, bool isConfirmed)
    {
        var payload = new Feedback
        {
            OccurrenceId = occurrenceId,
            UserId = userId,
            IsConfirmed = isConfirmed
        };
        return _client.PostAsJsonAsync("/api/feedback", payload);
    }

    /// <summary>
    /// Tests the create feedback method successfully.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var (occId, _) = await CreateOccurrenceAsync(activate: true);

        var resp = await PostFeedbackAsync(occId, userId: 1, isConfirmed: true);
        await AssertCreatedOrThrow(resp);

        var created = await resp.Content.ReadFromJsonAsync<Feedback>(_json);
        Assert.NotNull(created);
        Assert.Equal(occId, created!.OccurrenceId);
        Assert.Equal(1, created.UserId);
        Assert.True(created.IsConfirmed);
    }

    /// <summary>
    /// Tests the create feedback method when a duplicate is submitted within an hour.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDuplicateWithinHour()
    {
        var (occId, _) = await CreateOccurrenceAsync(activate: true);

        var first = await PostFeedbackAsync(occId, userId: 1, isConfirmed: false);
        await AssertCreatedOrThrow(first);

        var second = await PostFeedbackAsync(occId, userId: 1, isConfirmed: true);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        var body = await second.Content.ReadAsStringAsync();
        Assert.Contains("within the last hour", body, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests the create feedback method when the user is missing.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsNotFound_WhenUserMissing()
    {
        var (occId, _) = await CreateOccurrenceAsync(activate: true);

        var resp = await PostFeedbackAsync(occId, userId: 999999, isConfirmed: true);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Tests the create feedback method when the occurrence is missing.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsNotFound_WhenOccurrenceMissing()
    {
        var resp = await PostFeedbackAsync(occurrenceId: 999999, userId: 1, isConfirmed: false);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Tests the create feedback method when the occurrence is in waiting status.
    /// </summary>
    [Fact]
    public async Task Create_ReturnsInternalServerError_WhenOccurrenceWaiting()
    {
        var (occId, details) = await CreateOccurrenceAsync(activate: false);
        Assert.Equal(OccurrenceStatus.WAITING, details.Status);

        var resp = await PostFeedbackAsync(occId, userId: 1, isConfirmed: true);
        Assert.Equal(HttpStatusCode.InternalServerError, resp.StatusCode);
    }

    /// <summary>
    /// Tests the create feedback method when the request body is null.
    /// </summary>
    [Fact]
    public async Task Create_BadRequest_WhenBodyNull()
    {
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/feedback", content);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    /// <summary>
    /// Tests that submitting five negative feedbacks closes the occurrence.
    /// </summary>
    [Fact]
    public async Task Create_FiveNegativeFeedbacks_ClosesOccurrence()
    {
        var (occId, _) = await CreateOccurrenceAsync(activate: true);

        for (int userId = 1; userId <= 5; userId++)
        {
            var resp = await PostFeedbackAsync(occId, userId: userId, isConfirmed: false);
            await AssertCreatedOrThrow(resp);
        }

        var after = await _client.GetFromJsonAsync<OccurrenceDetailsDto>($"/api/occurrences/{occId}", _json);
        Assert.NotNull(after);
        Assert.Equal(OccurrenceStatus.CLOSED, after!.Status);
        Assert.True(after.EndDateTime != default);
    }
}
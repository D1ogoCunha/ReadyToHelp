namespace readytohelpapi.Report.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using readytohelpapi.Common.Data;
using readytohelpapi.Common.Tests;
using readytohelpapi.Report.DTOs;
using readytohelpapi.Report.Models;
using readytohelpapi.User.Models;
using readytohelpapi.ResponsibleEntity.Models;
using Xunit;


public partial class Program { }

[Trait("Category", "Integration")]
public class TestReportApiController_Integration : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TestReportApiController_Integration(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    ctx.Database.Migrate();
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
                {
                    ctx.Database.EnsureCreated();
                }

                services.AddHttpClient<readytohelpapi.Notifications.NotifierClient>()
                        .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpHandler());
            });
        });

        client = _factory.CreateClient();
    }

    private static string UniqueEmail() => $"it_{Guid.NewGuid():N}@example.com";

    private int SeedUser()
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new User { Name = "Reporter", Email = UniqueEmail(), Profile = Profile.CITIZEN, Password = "x" };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user.Id;
    }

    private void SeedResponsibleEntityCovering(double lat, double lon)
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var gf = new GeometryFactory(new PrecisionModel(), 4326);
        var coords = new[]
        {
            new Coordinate(lon - 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat + 0.01),
            new Coordinate(lon + 0.01, lat - 0.01),
            new Coordinate(lon - 0.01, lat - 0.01),
        };
        var poly = gf.CreatePolygon(coords);
        var multi = gf.CreateMultiPolygon(new[] { poly });

        var entity = new ResponsibleEntity
        {
            Name = "Entidade Teste",
            Email = "re@test.local",
            Address = "Rua X",
            ContactPhone = 123456789,
            Type = ResponsibleEntityType.BOMBEIROS,
            GeoArea = multi
        };

        ctx.ResponsibleEntities.Add(entity);
        ctx.SaveChanges();
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        var resp = await client.GetAsync("/api/reports/0");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var resp = await client.GetAsync("/api/reports/99999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_NullBody_ReturnsBadRequest()
    {
        var resp = await client.PostAsync("/api/reports", new StringContent("", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(doc);
        Assert.Equal("invalid_request", doc!.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Create_Unauthenticated_ReturnsUnauthorized()
    {
        using var unauthFactory = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureServices(s =>
            {
                s.AddAuthentication("NoAuth")
                 .AddScheme<AuthenticationSchemeOptions, NoAuthHandler>("NoAuth", _ => { });

                s.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = "NoAuth";
                    o.DefaultChallengeScheme = "NoAuth";
                });
            });
        });
        using var unauth = unauthFactory.CreateClient();

        var resp = await unauth.PostAsJsonAsync("/api/reports", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    private sealed class NoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public NoAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(AuthenticateResult.NoResult());
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ok = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(ok);
        }
    }
}
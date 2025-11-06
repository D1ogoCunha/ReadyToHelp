namespace readytohelpapi.Common.Tests;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using readytohelpapi.Common.Data;
using readytohelpapi.Notifications;

public partial class Program { }

/// <summary>
/// Web application factory for integration tests.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"report_int_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            var user = Environment.GetEnvironmentVariable("POSTGRES_USERNAME")
                       ?? Environment.GetEnvironmentVariable("POSTGRES_USER")
                       ?? "readytohelp";
            var pwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(
                    $"Host={host};Port={port};Database={DatabaseName};Username={user};Password={pwd}",
                    npgsql => npgsql.UseNetTopologySuite()
                )
            );

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "Test";
                o.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.RemoveAll<NotifierClient>();
            var http = new HttpClient(new OkHandler()) { BaseAddress = new Uri("http://localhost") };
            services.AddSingleton(new NotifierClient(http, NullLogger<NotifierClient>.Instance));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
namespace readytohelpapi.Report.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Data;

/// <summary>
///   Provides a test database fixture for report tests.
///   This fixture sets up a unique test database for each test run,
///   ensuring isolation and proper cleanup after tests.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string databaseName;

    /// <summary>
    ///  Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USERNAME")
                   ?? Environment.GetEnvironmentVariable("POSTGRES_USER")
                   ?? "readytohelp";
        var pwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        databaseName = $"report_test_db_{Guid.NewGuid():N}";

        var services = new ServiceCollection()
            .AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(
                    $"Host={host};Port={port};Database={databaseName};Username={user};Password={pwd}",
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
                )
            )
            .BuildServiceProvider();

        Context = services.GetRequiredService<AppDbContext>();
        Context.Database.EnsureCreated();
    }

    /// <summary>
    ///  Gets the AppDbContext instance for database operations in tests.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    ///  Resets the database by clearing tracked entities and removing all reports.
    /// </summary>
    public void ResetDatabase()
    {
        Context.ChangeTracker.Clear();
        var all = Context.Reports.AsNoTracking().ToList();
        if (all.Any())
        {
            Context.Reports.RemoveRange(all);
            Context.SaveChanges();
        }
        Context.ChangeTracker.Clear();
    }

    /// <summary>
    ///  Disposes of the database context and deletes the test database.
    /// </summary>
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
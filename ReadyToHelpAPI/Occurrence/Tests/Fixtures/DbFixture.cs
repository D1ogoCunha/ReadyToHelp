using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Data;

namespace readytohelpapi.Occurrence.Tests;

/// <summary>
///     Provides a test fixture for setting up and managing the OccurrenceContext database.
///     This fixture ensures the database is created, reset, and disposed properly
///     during the testing lifecycle.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string _databaseName;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        _databaseName = $"occ_tests_{Guid.NewGuid():N}";

        var sp = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    $"Host={postgresHost};Port={postgresPort};Database={_databaseName};Username={postgresUser};Password={postgresPwd}",
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
                )
            )
            .BuildServiceProvider();

        Context = sp.GetRequiredService<AppDbContext>();
        Context.Database.EnsureCreated();
    }

    /// <summary>
    ///   Gets the database context for the fixture.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    ///   Resets the database by clearing tracked entities and removing all occurrences.
    /// </summary>
    public void ResetDatabase()
    {
        Context.ChangeTracker.Clear();
        var all = Context.Occurrences.AsNoTracking().ToList();
        if (all.Any())
        {
            Context.Occurrences.RemoveRange(all);
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
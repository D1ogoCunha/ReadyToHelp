namespace readytohelpapi.Dashboard.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Data;

/// /// <summary>
///   Initializes a new instance of the <see cref="DbFixture"/> class.
///   Sets up the AppDbContext with a unique test database.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string databaseName;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd =
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        databaseName = $"dashboard_tests_{Guid.NewGuid():N}";

        var sp = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    $"Host={postgresHost};Port={postgresPort};Database={databaseName};Username={postgresUser};Password={postgresPwd}",
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
                )
            )
            .BuildServiceProvider();

        Context = sp.GetRequiredService<AppDbContext>();
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Gets the database context for the fixture.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    /// Remove data from all tables used by dashboard tests so each test run starts clean.
    /// </summary>
    public void ResetDatabase()
    {
        Context.ChangeTracker.Clear();

        if (Context.Feedbacks.Any())
        {
            Context.Feedbacks.RemoveRange(Context.Feedbacks.AsNoTracking().ToList());
            Context.SaveChanges();
        }

        if (Context.Reports.Any())
        {
            Context.Reports.RemoveRange(Context.Reports.AsNoTracking().ToList());
            Context.SaveChanges();
        }

        if (Context.Occurrences.Any())
        {
            Context.Occurrences.RemoveRange(Context.Occurrences.AsNoTracking().ToList());
            Context.SaveChanges();
        }

        if (Context.ResponsibleEntities.Any())
        {
            Context.ResponsibleEntities.RemoveRange(
                Context.ResponsibleEntities.AsNoTracking().ToList()
            );
            Context.SaveChanges();
        }

        if (Context.Users.Any())
        {
            Context.Users.RemoveRange(Context.Users.AsNoTracking().ToList());
            Context.SaveChanges();
        }

        Context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Disposes the database context and deletes the test database.
    /// </summary>
    /// <param name="disposing">Indicates whether the call is from Dispose (true) or from a finalizer (false).</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing && Context != null)
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            Context.Dispose();
        }
        disposed = true;
    }

    /// <summary>
    /// Disposes the database context and deletes the test database.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

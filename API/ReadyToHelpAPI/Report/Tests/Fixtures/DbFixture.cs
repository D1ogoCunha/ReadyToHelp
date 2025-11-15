namespace readytohelpapi.Report.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Data;

/// <summary>
///   Initializes a new instance of the <see cref="DbFixture"/> class.
///   Sets up the AppDbContext with a unique test database.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string databaseName;
    private bool disposed;

    /// <summary>
    ///  Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var user =
            Environment.GetEnvironmentVariable("POSTGRES_USERNAME")
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

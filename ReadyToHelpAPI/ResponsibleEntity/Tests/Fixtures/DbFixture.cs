namespace readytohelpapi.ResponsibleEntity.Tests.Fixtures;

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
    ///   Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd =
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";
        databaseName = $"responsibleentity_tests_{Guid.NewGuid():N}";

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
    ///   Gets the database context for the fixture.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    ///   Resets the database by clearing tracked entities and removing all occurrences.
    /// </summary>
    public void ResetDatabase()
    {
        Context.ChangeTracker.Clear();
        var all = Context.ResponsibleEntities.AsNoTracking().ToList();
        if (all.Any())
        {
            Context.ResponsibleEntities.RemoveRange(all);
            Context.SaveChanges();
        }
        Context.ChangeTracker.Clear();
    }

    /// <summary>
    ///  Disposes of the database context and deletes the test database.
    ///  Implements the dispose pattern for inheritable types.
    /// </summary>
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
    /// Releases all resources used by the <see cref="DbFixture"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

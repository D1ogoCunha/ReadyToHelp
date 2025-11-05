namespace readytohelpapi.Feedback.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Common.Data;
using System;
using System.Linq;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.User.Models;


/// <summary>
///  Fixture for setting up and managing the test database related to feedback.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string _databaseName;
    private bool disposed;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DbFixture"/> class.
    /// </summary>
    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        _databaseName = $"test_db_feedback_{Guid.NewGuid():N}";

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    $"Host={postgresHost};Port={postgresPort};Database={_databaseName};Username={postgresUser};Password={postgresPwd}",
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
                )
            )
            .BuildServiceProvider();

        this.Context = serviceProvider.GetRequiredService<AppDbContext>();
        this.Context.Database.EnsureCreated();
    }

    /// <summary>
    ///   The database context for tests.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    /// Clear tracked entities and remove all feedback-related data for a clean test start.
    /// </summary>
    public void ResetDatabase()
    {
        this.Context.ChangeTracker.Clear();

        var feedbacks = this.Context.Set<Feedback>().AsNoTracking().ToList();
        if (feedbacks.Any())
        {
            this.Context.Set<Feedback>().RemoveRange(feedbacks);
            this.Context.SaveChanges();
        }

        var occurrences = this.Context.Set<Occurrence>().AsNoTracking().ToList();
        if (occurrences.Any())
        {
            this.Context.Set<Occurrence>().RemoveRange(occurrences);
            this.Context.SaveChanges();
        }

        var users = this.Context.Set<User>().AsNoTracking().ToList();
        if (users.Any())
        {
            this.Context.Set<User>().RemoveRange(users);
            this.Context.SaveChanges();
        }

        this.Context.ChangeTracker.Clear();
    }

    /// <summary>
    ///  Disposes of the database context and deletes the test database.
    ///  Implements the dispose pattern for inheritable types.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing && Context != null)
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch
            {
                // Ignore exceptions during database deletion
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

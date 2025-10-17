using Microsoft.EntityFrameworkCore;
using readytohelpapi.User.Data;

namespace readytohelpapi.User.Tests;

/// <summary>
///     Provides a test fixture for setting up and managing the UserContext database.
///     This fixture ensures the database is created, reset, and disposed properly
///     during the testing lifecycle.
/// </summary>
public class DbFixture : IDisposable
{
    private readonly string _databaseName;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DbFixture"/> class.
    ///   Sets up the UserContext with a unique test database.
    /// </summary>
    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd =
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        _databaseName = $"test_db_{Guid.NewGuid():N}";

        var serviceProvider = new ServiceCollection()
            .AddDbContext<UserContext>(options =>
                options.UseNpgsql(
                    $"Host={postgresHost};Port={postgresPort};Database={_databaseName};Username={postgresUser};Password={postgresPwd}"
                )
            )
            .BuildServiceProvider();

        this.Context = serviceProvider.GetRequiredService<UserContext>();
        this.Context.Database.EnsureCreated();
    }

    /// <summary>
    ///   Gets the UserContext instance for database operations in tests.
    /// </summary>
    public UserContext Context { get; }

    /// <summary>
    ///  Disposes of the database context and deletes the test database.
    /// </summary>
    public void Dispose()
    {
        this.Context.Database.EnsureDeleted();
        this.Context.Dispose();
    }
    /// <summary>
    ///   Resets the database by clearing tracked entities and removing all users.
    /// </summary>
    public void ResetDatabase()
    {
        this.Context.ChangeTracker.Clear();
        var users = this.Context.Users.AsNoTracking().ToList();
        if (users.Any())
        {
            this.Context.Users.RemoveRange(users);
            this.Context.SaveChanges();
        }
        this.Context.ChangeTracker.Clear();
    }
}

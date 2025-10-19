using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readytohelpapi.Occurrence.Data;

namespace readytohelpapi.Occurrence.Tests;

public class DbFixture : IDisposable
{
    private readonly string _databaseName;

    public DbFixture()
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
        var postgresPwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";

        _databaseName = $"occ_tests_{Guid.NewGuid():N}";

        var sp = new ServiceCollection()
            .AddDbContext<OccurrenceContext>(options =>
                options.UseNpgsql(
                    $"Host={postgresHost};Port={postgresPort};Database={_databaseName};Username={postgresUser};Password={postgresPwd}"
                ))
            .BuildServiceProvider();

        Context = sp.GetRequiredService<OccurrenceContext>();
        Context.Database.EnsureCreated();
    }

    public OccurrenceContext Context { get; }

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

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
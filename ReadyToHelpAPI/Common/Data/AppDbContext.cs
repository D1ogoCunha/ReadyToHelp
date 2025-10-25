namespace readytohelpapi.Common.Data;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.User.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;
using readytohelpapi.Feedback.Models;
using readytohelpapi.ResponsibleEntity.Models;

/// <summary>
///  Defines the application database context.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public AppDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var user = Environment.GetEnvironmentVariable("POSTGRES_USERNAME")
                      ?? Environment.GetEnvironmentVariable("POSTGRES_USER")
                      ?? "readytohelp";
            var pwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";
            var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "readytohelp_db";
            var conn = $"Host={host};Port=5432;Database={db};Username={user};Password={pwd}";

            optionsBuilder.UseNpgsql(conn, o => o.UseNetTopologySuite());
        }
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Occurrence> Occurrences { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ResponsibleEntity> ResponsibleEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(120).IsRequired();
            entity.Property(u => u.Password).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Profile).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Occurrence>(entity =>
        {
            entity.ToTable("occurrences");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Title).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Description).HasMaxLength(1000).IsRequired();
            entity.Property(o => o.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.Priority).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.ProximityRadius).IsRequired();
            entity.Property(o => o.CreationDateTime).IsRequired();
            entity.Property(o => o.EndDateTime);
            entity.Property(o => o.ReportCount).IsRequired();

            entity.OwnsOne(o => o.Location, nb =>
            {
                nb.Property(p => p.Latitude).HasColumnName("Latitude");
                nb.Property(p => p.Longitude).HasColumnName("Longitude");
            });

            entity.HasOne<Report>()
                  .WithMany()
                  .HasForeignKey(o => o.ReportId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_occurrences_report");

            entity.HasOne<ResponsibleEntity>()
                  .WithMany()
                  .HasForeignKey(o => o.ResponsibleEntityId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_occurrences_responsible_entity");
        });

        modelBuilder.Entity<Report>(b =>
        {
            b.ToTable("reports");
            b.HasKey(r => r.Id);
            b.Property(r => r.Title).IsRequired().HasMaxLength(200);
            b.Property(r => r.Description).IsRequired().HasMaxLength(1000);
            b.Property(r => r.ReportDateTime).IsRequired();

            b.OwnsOne(r => r.Location, nb =>
            {
                nb.Property(p => p.Latitude).HasColumnName("Latitude");
                nb.Property(p => p.Longitude).HasColumnName("Longitude");
            });

            b.HasOne<User>()
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade)
             .HasConstraintName("FK_reports_user");

            b.Ignore(r => r.Status);
            b.Ignore(r => r.Priority);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedback");
            entity.HasKey(f => f.Id).HasName("PK_feedback");
            entity.Property(f => f.FeedbackDateTime).IsRequired();
            entity.Property(f => f.IsConfirmed).IsRequired();

            entity.HasOne<Occurrence>()
                  .WithMany()
                  .HasForeignKey(f => f.OccurrenceId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_feedback_occurrence");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_feedback_user");

            entity.HasIndex(f => f.OccurrenceId);
            entity.HasIndex(f => f.UserId);
        });

        modelBuilder.Entity<ResponsibleEntity>(entity =>
        {
            entity.ToTable("responsible_entities");
            entity.HasKey(re => re.Id).HasName("PK_responsible_entities");
            entity.Property(re => re.Name).IsRequired().HasMaxLength(200);
            entity.Property(re => re.Email).IsRequired().HasMaxLength(200);
            entity.Property(re => re.ContactPhone).HasMaxLength(500);
            entity.Property(re => re.Address).HasMaxLength(500);
            entity.Property(re => re.Type).HasConversion<string>().HasMaxLength(100);
            entity.Property(e => e.GeoArea)
                .HasColumnType("geometry(MultiPolygon,4326)");
        });
    }
}
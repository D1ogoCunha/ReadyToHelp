namespace readytohelpapi.Report.Data;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.Report.Models;
using readytohelpapi.GeoPoint.Models;

/// <summary>
///     Represents the database context for report-related operations.
/// </summary>
public class ReportContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReportContext" /> class.
    /// </summary>
    /// <param name="options">The options for the database context.</param>
    public ReportContext(DbContextOptions<ReportContext> options)
        : base(options) { }

    /// <summary>
    ///     Gets or sets the Reports DbSet.
    /// </summary>
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Report>(b =>
        {
            b.ToTable("Reports");
            b.HasKey(r => r.Id);

            b.Property(r => r.Title).IsRequired().HasMaxLength(200);
            b.Property(r => r.Description).IsRequired().HasMaxLength(1000);
            b.Property(r => r.ReportDateTime).IsRequired();
            b.Property(r => r.IsDuplicate).HasDefaultValue(false);
            b.Property(r => r.UserId).IsRequired();

            b.OwnsOne(r => r.Location, nb =>
            {
                nb.Property(p => p.Latitude).HasColumnName("Latitude");
                nb.Property(p => p.Longitude).HasColumnName("Longitude");
            });

            b.HasIndex(r => new { r.UserId, r.ReportDateTime });
        });
    }
}
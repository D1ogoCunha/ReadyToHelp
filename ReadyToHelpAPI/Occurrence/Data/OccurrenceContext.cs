using readytohelpapi.Occurrence.Models;
using Microsoft.EntityFrameworkCore;

namespace readytohelpapi.Occurrence.Data;

/// <summary>
///    Represents the database context for occurrence-related operations.
/// </summary>
public class OccurrenceContext : DbContext
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="OccurrenceContext" /> class.
    /// </summary>
    /// <param name="options">The options for the database context.</param>
    public OccurrenceContext(DbContextOptions<OccurrenceContext> options)
        : base(options) { }

    /// <summary>
    ///  Gets or sets the Occurrences DbSet.
    /// </summary>
    public DbSet<Models.Occurrence> Occurrences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Occurrence>(entity =>
        {
            entity.ToTable("occurrences");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Title)
                  .HasMaxLength(200)
                  .IsRequired();
            entity.Property(o => o.Description)
                  .HasMaxLength(1000)
                  .IsRequired();
            entity.Property(o => o.Type)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            entity.Property(o => o.Status)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            entity.Property(o => o.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            entity.Property(o => o.ProximityRadius)
                  .IsRequired();
            entity.Property(o => o.CreationDateTime)
                  .IsRequired();
            entity.Property(o => o.EndDateTime);
            entity.Property(o => o.ReportCount)
                  .IsRequired();
            entity.Property(o => o.ReportId);
            entity.Property(o => o.ResponsibleEntityId);

            entity.OwnsOne(o => o.Location, nb =>
            {
                nb.Property(p => p.Latitude).HasColumnName("Latitude");
                nb.Property(p => p.Longitude).HasColumnName("Longitude");
            });
        });
    }
}
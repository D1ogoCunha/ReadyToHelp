using Microsoft.EntityFrameworkCore;
using readytohelpapi.Report.Models;
using readytohelpapi.GeoPoint.Models;

namespace readytohelpapi.Report.Data;

public class ReportContext : DbContext
{
    public ReportContext(DbContextOptions<ReportContext> options) : base(options) { }

    public DbSet<Models.Report> Reports => Set<Models.Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Report>(b =>
        {
            b.ToTable("Reports");
            b.HasKey(r => r.Id);

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
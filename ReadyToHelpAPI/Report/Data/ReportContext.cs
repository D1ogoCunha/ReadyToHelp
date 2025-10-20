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
}
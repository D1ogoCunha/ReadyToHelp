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
}
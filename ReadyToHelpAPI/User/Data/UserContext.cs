namespace readytohelpapi.User.Data;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.User.Models;

/// <summary>
///     Represents the database context for user-related operations.
/// / </summary>
public class UserContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserContext" /> class.
    /// </summary>
    /// <param name="options">The options for the database context.</param>
    public UserContext(DbContextOptions<UserContext> options)
        : base(options) { }

    /// <summary>
    ///     Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<Models.User> Users { get; set; }
}
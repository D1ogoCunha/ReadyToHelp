// <copyright file="UserContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using readytohelpapi.User.Models;
using Microsoft.EntityFrameworkCore;

namespace readytohelpapi.User.Data;

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
            protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name)
                      .HasMaxLength(100)
                      .IsRequired();
                entity.Property(u => u.Email)
                      .HasMaxLength(120)
                      .IsRequired();
                entity.Property(u => u.PasswordHash)
                      .HasMaxLength(200)
                      .IsRequired();
                entity.Property(u => u.Profile)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });
        }

}
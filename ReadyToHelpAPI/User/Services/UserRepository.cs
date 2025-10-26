namespace readytohelpapi.User.Services;

using readytohelpapi.User.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using readytohelpapi.Common.Data;

/// <summary>
///     Implements the user repository operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext userContext;

    /// <summary>
    ///    Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The user database context.</param>
    public UserRepository(AppDbContext context)
    {
        userContext = context;
    }

    /// <inheritdoc />
    public User Create(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        try
        {
            var created = userContext.Users.Add(user).Entity;
            userContext.SaveChanges();
            return created;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to create user", ex);
        }
    }
    /// <inheritdoc />
    public User Update(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        try
        {
            userContext.Users.Update(user);
            userContext.SaveChanges();
            return user;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to update user", ex);
        }
    }

    /// <inheritdoc />
    public User? Delete(int id)
    {
        var existing = userContext.Users.Find(id);
        if (existing == null) return null;
        try
        {
            userContext.Users.Remove(existing);
            userContext.SaveChanges();
            return existing;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to delete user", ex);
        }
    }

    /// <inheritdoc />
    public User? GetUserById(int id)
    {
        if (id <= 0) return null;
        return userContext.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == id);
    }

    /// <inheritdoc />
    public User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return userContext.Users
            .AsNoTracking()
            .FirstOrDefault(u => EF.Functions.ILike(u.Email, email.Trim()));
    }

    /// <inheritdoc />
    public List<User> GetUserByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<User>();

        var pattern = $"%{name.Trim()}%";
        return userContext.Users
            .AsNoTracking()
            .Where(u => EF.Functions.ILike(u.Name, pattern))
            .ToList();
    }

    /// <inheritdoc />
    public List<User> GetAllUsers(int pageNumber = 1, int pageSize = 10, string sortBy = "Name", string sortOrder = "asc", string filter = "")
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var query = userContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var trimmed = filter.Trim();
            var pattern = $"%{trimmed}%";

            query = query.Where(u =>
                EF.Functions.ILike(u.Name ?? string.Empty, pattern) ||
                EF.Functions.ILike(u.Email ?? string.Empty, pattern));
        }

        var asc = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sortBy?.ToLowerInvariant())
        {
            case "name":
                query = asc ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name);
                break;
            case "email":
                query = asc ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                break;
            default:
                query = asc ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id);
                break;
        }

        var skip = (pageNumber - 1) * pageSize;
        return query.Skip(skip).Take(pageSize).ToList();
    }

}
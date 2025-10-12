namespace readytohelpapi.User.Services;

using readytohelpapi.User.Data;
using readytohelpapi.User.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class UserRepository : IUserRepository
{
    private readonly UserContext userContext;

    public UserRepository(UserContext context)
    {
        userContext = context;
    }   

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

    public User? GetProfile(int id)
    {
        return userContext.Users.Find(id);
    }

    public User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return userContext.Users
            .AsNoTracking()
            .FirstOrDefault(u => EF.Functions.ILike(u.Email, email.Trim()));
    }
}
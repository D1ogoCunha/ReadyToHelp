namespace readytohelpapi.User.Services;

using Models;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     Implements the user service operations.
/// </summary>
public class UserServiceImpl : IUserService
{
    private readonly IUserRepository userRepository;

    /// <summary>
    ///    Initializes a new instance of the <see cref="UserServiceImpl"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository instance.</param>
    public UserServiceImpl(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <inheritdoc />
    public User Create(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User object is null");

        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrEmpty(user.Email))
            throw new ArgumentException("Email cannot be null or empty", nameof(user.Email));

        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrEmpty(user.Name))
            throw new ArgumentException("User name cannot be null or empty", nameof(user.Name));

        if (string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrEmpty(user.Password))
            throw new ArgumentException("User password cannot be null or empty", nameof(user.Password));

        if (!Enum.IsDefined(typeof(Profile), user.Profile))
            throw new ArgumentOutOfRangeException(nameof(user.Profile), "Invalid profile");

        var existingUsers = this.userRepository.GetUserByEmail(user.Email);

        if (existingUsers != null)
            throw new ArgumentException($"Email {user.Email} already exists.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        try
        {
            return this.userRepository.Create(user);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to create a user.", e);
        }
    }

    /// <inheritdoc />
    public User Update(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null");

        if (user.Id <= 0)
            throw new ArgumentException("Invalid user id.");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("User email object is null or empty.");

        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ArgumentException("User name cannot be null or empty");

        var existingUser = this.userRepository.GetUserById(user.Id);
        if (existingUser == null)
            throw new KeyNotFoundException($"User with id {user.Id} not found.");

        if (string.IsNullOrWhiteSpace(user.Password))
        {
            user.Password = existingUser.Password;
        }
        else
        {
            bool sameAsExisting = false;
            if (!string.IsNullOrEmpty(existingUser.Password))
            {
                try
                {
                    sameAsExisting = BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    sameAsExisting = false;
                }
            }

            user.Password = sameAsExisting
                ? existingUser.Password
                : BCrypt.Net.BCrypt.HashPassword(user.Password);
        }

        var emailExists = this.userRepository.GetUserByEmail(user.Email);
        if (emailExists != null && emailExists.Id != user.Id)
            throw new ArgumentException($"Email {user.Email} already exists.");

        try
        {
            return this.userRepository.Update(user);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to update a user.", e);
        }
    }

    /// <inheritdoc />
    public User Delete(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Invalid user id.");
        }

        try
        {
            return this.userRepository.Delete(id);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to delete a user.", e);
        }
    }

    /// <inheritdoc />
    public User GetUserById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid user id.");

        var user = this.userRepository.GetUserById(id);
        if (user == null)
            throw new KeyNotFoundException($"User with id {id} not found.");

        return user;
    }

    /// <inheritdoc />
    public List<User> GetUserByName(string name)
    {
        return this.userRepository.GetUserByName(name);
    }

    /// <inheritdoc />
    public List<User> GetAllUsers(int pageNumber, int pageSize, string sortBy, string sortOrder, string filter)
    {
        if (string.IsNullOrEmpty(sortBy))
            throw new ArgumentException("Sort field cannot be null or empty.", nameof(sortBy));

        if (sortOrder != "asc" && sortOrder != "desc")
            throw new ArgumentException("Sort order must be 'asc' or 'desc'.", nameof(sortOrder));

        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000.", nameof(pageSize));

        try
        {
            var users = this.userRepository.GetAllUsers(pageNumber, pageSize, sortBy, sortOrder, filter);
            return users ?? new List<User>();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while retrieving users.", e);
        }
    }



    /// <inheritdoc />
    public Models.User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        return userRepository.GetUserByEmail(email);
    }

    /// <inheritdoc />
    public Models.User Register(Models.User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User object is null");

        user.Profile = Profile.CITIZEN;

        return Create(user);
    }
}
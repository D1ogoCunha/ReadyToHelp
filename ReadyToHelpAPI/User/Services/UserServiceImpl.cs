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

    /// <summary>
    ///   Creates a user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created user.</returns>
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

    /// <summary>
    ///   Updates a user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the user is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the user has invalid properties.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an error occurs while updating the user.</exception>
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

    /// <summary>
    ///   Deletes a user by id.
    /// </summary>
    /// <param name="id">The id of the user to delete.</param>
    /// <returns>The deleted user.</returns>
    /// <exception cref="ArgumentException">Thrown when the user id is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an error occurs while deleting the user.</exception>
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

    /// <summary>
    ///  Gets a user by id.
    /// </summary>
    /// <param name="id">The id of the user to get.</param>
    /// <returns>The user with the specified id.</returns>
    /// <exception cref="ArgumentException">Thrown when the user id is invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    public User GetUserById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid user id.");

        var user = this.userRepository.GetUserById(id);
        if (user == null)
            throw new KeyNotFoundException($"User with id {id} not found.");

        return user;
    }

    /// <summary>
    /// Gets users by name.
    /// </summary>
    /// <param name="name">The name of the users to get.</param>
    /// <returns>A list of users with the specified name.</returns>
    public List<User> GetUserByName(string name)
    {
        return this.userRepository.GetUserByName(name);
    }

    /// <summary>
    /// Gets all users with pagination, sorting, and filtering.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <param name="sortBy">The field to sort by.</param>
    /// <param name="sortOrder">The sort order (asc or desc).</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>A list of users matching the criteria.</returns>
    /// <exception cref="ArgumentException">Thrown when any of the parameters are invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an error occurs while retrieving users.</exception>
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



    /// <summary>
    ///   Gets a user by email.
    /// </summary>
    /// <param name="email">The email of the user to get.</param>
    /// <returns>The user with the specified email, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when the email is null or empty.</exception>

    public Models.User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        return userRepository.GetUserByEmail(email);
    }

    /// <summary>
    ///   Registers a new citizen user.
    /// </summary>
    /// <param name="user">The user to register.</param>
    /// <returns>The registered user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the user is null.</exception>
    public Models.User Register(Models.User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User object is null");

        user.Profile = Profile.CITIZEN;

        return Create(user);
    }
}
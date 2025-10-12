namespace readytohelpapi.User.Services;

using Models;
using System.Collections.Generic;

/// <summary>
///     Defines the contract for a user repository to manage data.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    ///     Creates a user in the repository.
    /// </summary>
    /// <param name="user">The user object to be created.</param>
    /// <returns>The created user entity.</returns>
    Models.User Create(User user);

    /// <summary>
    ///    Retrieves a user profile by ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Models.User? GetProfile(int id);

    /// <summary>
    ///   Retrieves users by email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>A list of users with the specified email.</returns>
    Models.User? GetUserByEmail(string email);
}
namespace readytohelpapi.User.Services;

using readytohelpapi.User.Models;

/// <summary>
///     Defines the contract for the user service operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Creates a user.
    /// </summary>
    /// <param name="user">The user object to be created.</param>
    /// <returns>The created user entity.</returns>
    Models.User Create(User user);

    /// <summary>
    ///     Retrieves a user profile by ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Models.User? GetProfile(int id);
}

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
    User Create(User user);

    /// <summary>
    ///     Updates a user.
    /// </summary>
    /// <param name="user">The user object to be updated.</param>
    /// <returns>The updated user entity.</returns>
    User Update(User user);

    /// <summary>
    ///     Deletes a user.
    /// </summary>
    /// <param name="id">The user id to be deleted.</param>
    /// <returns>The deleted user entity, if successfully found.</returns>
    User Delete(int id);

    /// <summary>
    ///     Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user entity if found.</returns>
    User GetUserById(int id);

    /// <summary>
    ///     Retrieves a list of users by partial name.
    /// </summary>
    /// <param name="name">The partial or full name to search for.</param>
    /// <returns>A list of users that match the search criteria.</returns>
    List<User> GetUserByName(string name);

    /// <summary>
    ///     Retrieves a paginated, filtered, and sorted list of users.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field by which to sort the results.</param>
    /// <param name="sortOrder">The sort order, either "asc" or "desc".</param>
    /// <param name="filter">The string to filter the user data.</param>
    /// <returns>A paginated, sorted, and filtered list of users.</returns>
    List<User> GetAllUsers(
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortOrder,
        string filter
    );

    /// <summary>
    ///     Retrieves a user by email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    User? GetUserByEmail(string email);

    /// <summary>
    /// Registers for a new user in the mobile app, forces the user was a CITIZEN.
    /// </summary>
    /// <param name="user">The user object containing registration details.</param>
    User Register(User user);
}

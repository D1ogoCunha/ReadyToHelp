namespace readytohelpapi.User.Services;

using Models;
using System.Collections.Generic;
using System;

//// <summary>
///     Defines the contract for a user repository to manage data.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    ///     Creates a user in the repository.
    /// </summary>
    /// <param name="user">The user object to be created.</param>
    /// <returns>The created user entity.</returns>s
    Models.User Create(User user);

    /// <summary>
    ///    Retrieves a user profile by ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Models.User? GetProfile(int id);

    /// <summary>
    ///    Retrieves a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Models.User? GetUserById(int id);

    /// <summary>
    ///    Updates a user in the repository.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user entity.</returns>
    Models.User Update(User user);

    /// <summary>
    ///    Deletes a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The deleted user entity if found; otherwise, null.</returns>
    Models.User? Delete(int id);

    /// <summary>
    ///   Retrieves users by partial or full name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>A list of users that match the name.</returns>
    List<Models.User> GetUserByName(string name);

    /// <summary>
    ///   Retrieves the user by email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>The user with the specified email.</returns>
    Models.User? GetUserByEmail(string email);

    /// <summary>
    ///     Retrieves a paginated, filtered, and sorted list of users.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field by which to sort the results.</param>
    /// <param name="sortOrder">The sort order, either "asc" or "desc".</param>
    /// <param name="filter">The string to filter the user data.</param>
    /// <returns>A paginated, sorted, and filtered list of users.</returns>
    List<Models.User> GetAllUsers(int pageNumber, int pageSize, string sortBy, string sortOrder, string filter);
}
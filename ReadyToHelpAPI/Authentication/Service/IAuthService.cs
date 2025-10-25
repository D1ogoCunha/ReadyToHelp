using readytohelpapi.Authentication.Models;

namespace readytohelpapi.Authentication.Service;

/// <summary>
/// Defines the contract for authentication services, including user login and token refresh.
/// Inherits from IUserAuthService to include user authentication methods.
/// </summary>
public interface IAuthService : IUserAuthService
{
    /// <summary>
    /// Validates an existing JWT and, if valid, returns a new token with updated expiration.
    /// Returns empty string if validation fails.
    /// </summary>
    string RefreshToken(string existingToken);
}
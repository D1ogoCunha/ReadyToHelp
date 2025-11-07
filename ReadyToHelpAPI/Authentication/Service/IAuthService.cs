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
    /// <param name="existingToken">The existing JWT token.</param>
    /// <returns>New JWT token string or empty string if invalid/expired.</returns>
    string RefreshToken(string existingToken);

    /// <summary>
    /// Revokes the specified token, rendering it invalid for future use.
    /// </summary>
    /// <param name="token">The JWT token to revoke.</param>
    void RevokeToken(string token);
}
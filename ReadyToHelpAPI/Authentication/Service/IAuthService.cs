using readytohelpapi.Authentication.Models;

namespace readytohelpapi.Authentication.Service;

public interface IAuthService : IUserAuthService
{
    /// <summary>
    /// Validates an existing JWT and, if valid, returns a new token with updated expiration.
    /// Returns empty string if validation fails.
    /// </summary>
    string RefreshToken(string existingToken);
}
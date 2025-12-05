namespace readytohelpapi.Authentication.Service;

using readytohelpapi.Authentication.Models;

/// <summary>
/// Defines the contract for user authentication services, including login methods for mobile and web.
/// </summary>
public interface IUserAuthService
{
    /// <summary>
    /// Authenticates a user for mobile access and returns a JWT if successful.
    /// </summary>
    /// <param name="authentication">The authentication model containing email and password.</param>
    /// <returns>JWT token string.</returns>
    string UserLoginMobile(Authentication authentication);

    /// <summary>
    /// Authenticates a user for web access and returns a JWT if successful.
    /// </summary>
    /// <param name="authentication">The authentication model containing email and password.</param>
    /// <returns>JWT token string.</returns>
    string UserLoginWeb(Authentication authentication);
}

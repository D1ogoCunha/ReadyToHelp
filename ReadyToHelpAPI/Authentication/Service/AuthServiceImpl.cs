using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using readytohelpapi.Authentication.Models;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;

namespace readytohelpapi.Authentication.Service;

/// <summary>
/// Implementation of the authentication service.
/// </summary>
public class AuthServiceImpl : IAuthService
{
    private readonly IUserService userService;
    private readonly IConfiguration configuration;
    private readonly JwtSecurityTokenHandler tokenHandler = new();
    private readonly IDistributedCache cache;
    private IUserService @object;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthServiceImpl"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="configuration">The configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if JWT settings are not configured.</exception>
    public AuthServiceImpl(IUserService userService, IConfiguration configuration, IDistributedCache cache)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

        _ = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");
        _ = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured");
        _ = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured");
    }

    /// <summary>
    ///  Logs in a user for mobile access.
    ///  Validates the provided email and password.
    /// </summary>
    /// <param name="authentication">The authentication model containing email and password.</param>
    /// <returns>JWT token string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the authentication model is null.</exception>
    /// <exception cref="ArgumentException">Thrown if email or password is empty.</exception>
    /// <exception cref="AuthenticationException">Thrown if login credentials are invalid.</exception>
    public string UserLoginMobile(Models.Authentication authentication)
    {
        if (authentication is null) throw new ArgumentNullException(nameof(authentication));
        if (string.IsNullOrWhiteSpace(authentication.Email) || string.IsNullOrWhiteSpace(authentication.Password))
            throw new ArgumentException("Email and Password are required.");

        var user = userService.GetUserByEmail(authentication.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(authentication.Password, user.Password))
            throw new AuthenticationException("Invalid login credentials.");

        return TokenProvider(user.Id, user.Email, user.Profile);
    }

    /// <summary>
    /// Logs in a user for web access.
    /// Validates the provided email and password.
    /// Only users with ADMIN or MANAGER profiles are allowed.
    /// </summary>
    /// <param name="authentication">The authentication model containing email and password.</param>
    /// <returns>JWT token string.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user profile is not allowed for web login.</exception>
    /// <exception cref="AuthenticationException">Thrown if login credentials are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown if email or password is missing.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the authentication model is null.</exception>
    public string UserLoginWeb(Models.Authentication authentication)
    {
        if (authentication is null) throw new ArgumentNullException(nameof(authentication));
        if (string.IsNullOrWhiteSpace(authentication.Email) || string.IsNullOrWhiteSpace(authentication.Password))
            throw new ArgumentException("Email and Password are required.");

        var user = userService.GetUserByEmail(authentication.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(authentication.Password, user.Password))
            throw new AuthenticationException("Invalid login credentials.");

        if (user.Profile != Profile.ADMIN && user.Profile != Profile.MANAGER)
            throw new UnauthorizedAccessException("User profile not allowed for web login.");

        return TokenProvider(user.Id, user.Email, user.Profile);
    }

    /// <summary>
    /// Refreshes a JWT token if it is valid and not expired.
    /// Returns an empty string if the token is invalid or expired.
    /// </summary>
    /// <param name="existingToken">The existing JWT token.</param>
    /// <returns>New JWT token string or empty string if invalid/expired.</returns> 
    public string RefreshToken(string existingToken)
    {
        if (string.IsNullOrWhiteSpace(existingToken)) return string.Empty;
        try
        {
            var principal = ValidateToken(existingToken, out var jwtToken);
            var sub = jwtToken.Subject ?? string.Empty;
            if (!int.TryParse(sub, out var id)) return string.Empty;

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            if (string.IsNullOrWhiteSpace(roleClaim) || !Enum.TryParse<Profile>(roleClaim, true, out var profile)) return string.Empty;

            return TokenProvider(id, email, profile);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Revokes the specified token, rendering it invalid for future use.
    /// </summary>
    /// <param name="token">The JWT token to revoke.</param>
    public void RevokeToken(string token)
    {
        var jwt = JwtUtility.ConvertJwtStringToJwtSecurityToken(token);
        if (jwt == null) return;

        var jti = jwt.Id;
        if (string.IsNullOrWhiteSpace(jti)) return;

        var expiration = jwt.ValidTo;
        var totalTime = expiration - DateTime.UtcNow;
        if (totalTime <= TimeSpan.Zero) return;

        var key = $"revoked_tokens:{jti}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = totalTime
        };

        cache.SetString(key, JsonSerializer.Serialize(new { revokedAt = DateTime.UtcNow }), options);
    }

    /// <summary>
    /// Validates a JWT token and returns the associated ClaimsPrincipal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="jwtToken">The validated JWT token.</param>
    /// <returns>The ClaimsPrincipal associated with the token.</returns>
    /// <exception cref="SecurityTokenException"></exception>
    private ClaimsPrincipal ValidateToken(string token, out JwtSecurityToken jwtToken)
    {
        var keyBytes = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);
        jwtToken = securityToken as JwtSecurityToken ?? throw new SecurityTokenException("Invalid token");

        var jti = jwtToken.Id;
        if (!string.IsNullOrWhiteSpace(jti))
        {
            var key = $"revoked_tokens:{jti}";
            var revokedToken = cache.GetString(key);
            if (revokedToken != null)
            {
                throw new SecurityTokenException("Token has been revoked.");
            }
        }

        return principal;
    }

    /// <summary>
    /// Generates a JWT token for the specified user details.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="email">The user email.</param>
    /// <param name="profile">The user profile.</param>
    private string TokenProvider(int id, string email, Profile profile)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, profile.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var expiresDays = int.TryParse(configuration["Jwt:AccessTokenExpirationDays"], out var d) ? d : 1;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(expiresDays),
            SigningCredentials = creds,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }
}
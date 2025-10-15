using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using readytohelpapi.Authentication.Models;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;

namespace readytohelpapi.Authentication.Service;

public class AuthServiceImpl : IAuthService
{
    private readonly IUserService userService;
    private readonly IConfiguration configuration;
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    public AuthServiceImpl(IUserService userService, IConfiguration configuration)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _ = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");
        _ = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured");
        _ = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured");
    }

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

    public string RefreshToken(string existingToken)
    {
        if (string.IsNullOrWhiteSpace(existingToken)) return string.Empty;

        try
        {
            var principal = ValidateToken(existingToken, out var jwtToken); // valida lifetime
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
        return principal;
    }

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
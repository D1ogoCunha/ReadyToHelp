using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthDto = readytohelpapi.Authentication.Models.Authentication;
using AuthException = System.Security.Authentication.AuthenticationException;
using readytohelpapi.Authentication.Service;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using Xunit;

namespace ReadyToHelpAPI.Tests.Authentication;

/// <summary>
///     Unit tests for <see cref="AuthServiceImpl" />.
/// </summary>
public class TestAuthService
{
    private readonly Mock<IUserService> mockUserService;
    private readonly IConfiguration configuration;
    private readonly AuthServiceImpl authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthService"/> class.
    /// Sets up mock dependencies and configuration for testing.
    /// </summary>
    public TestAuthService()
    {
        mockUserService = new Mock<IUserService>();

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "0123456789ABCDEF0123456789ABCDEF",
            ["Jwt:Issuer"] = "ReadyToHelp",
            ["Jwt:Audience"] = "ReadyToHelpUsers",
            ["Jwt:AccessTokenExpirationDays"] = "1"
        };
        configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        authService = new AuthServiceImpl(mockUserService.Object, configuration);
    }

    /// <summary>
    ///   Helper method to create a JWT token with custom claims for testing.
    /// </summary>
    /// <param name="claims">Dictionary of claims to include in the token.</param>
    /// <param name="expiresDays">Number of days until the token expires.</param>
    /// <returns>Generated JWT token as a string.</returns>
    private string CreateCustomJwt(Dictionary<string, string> claims, int expiresDays = 1)
    {
        var secret = configuration["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimList = claims.Select(kv => new Claim(kv.Key, kv.Value)).ToList();

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claimList,
            expires: DateTime.UtcNow.AddDays(expiresDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Tests successful web login for valid credentials and allowed roles.
    /// </summary>
    [Fact]
    public void UserLoginWeb_ShouldReturnToken_WhenCredentialsValidAndRoleAllowed()
    {
        var user = new User(1, "Admin", "admin@mail.com", BCrypt.Net.BCrypt.HashPassword("1234"), Profile.ADMIN);
        mockUserService.Setup(u => u.GetUserByEmail("admin@mail.com")).Returns(user);

        var auth = new AuthDto("admin@mail.com", "1234");

        var token = authService.UserLoginWeb(auth);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Contains(".", token);
    }

    /// <summary>
    /// Tests unauthorized access when a user with a non-allowed role tries to log in for web access.
    /// </summary>
    [Fact]
    public void UserLoginWeb_ShouldThrowUnauthorized_WhenCitizenTriesToLogin()
    {
        var user = new User(2, "Citizen", "citizen@mail.com", BCrypt.Net.BCrypt.HashPassword("1234"), Profile.CITIZEN);
        mockUserService.Setup(u => u.GetUserByEmail("citizen@mail.com")).Returns(user);

        var auth = new AuthDto("citizen@mail.com", "1234");

        Assert.Throws<UnauthorizedAccessException>(() => authService.UserLoginWeb(auth));
    }

    /// <summary>
    /// Tests invalid login attempts with incorrect passwords.
    /// </summary>
    [Fact]
    public void UserLoginWeb_ShouldThrowAuthenticationException_WhenPasswordInvalid()
    {
        var user = new User(3, "Admin", "admin@mail.com", BCrypt.Net.BCrypt.HashPassword("correctpass"), Profile.ADMIN);
        mockUserService.Setup(u => u.GetUserByEmail("admin@mail.com")).Returns(user);

        var auth = new AuthDto("admin@mail.com", "wrongpass");

        Assert.Throws<AuthException>(() => authService.UserLoginWeb(auth));
    }

    /// <summary>
    /// Tests successful mobile login for valid credentials.
    /// </summary>
    [Fact]
    public void UserLoginMobile_ShouldReturnToken_ForValidUser()
    {
        var user = new User(4, "Citizen", "citizen@mail.com", BCrypt.Net.BCrypt.HashPassword("abcd"), Profile.CITIZEN);
        mockUserService.Setup(u => u.GetUserByEmail("citizen@mail.com")).Returns(user);

        var auth = new AuthDto("citizen@mail.com", "abcd");

        var token = authService.UserLoginMobile(auth);

        Assert.NotNull(token);
        Assert.Contains(".", token);
    }

    /// <summary>
    /// Tests refreshing a valid JWT token.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnNewToken_WhenTokenValid()
    {
        var user = new User(5, "Admin", "admin@mail.com", BCrypt.Net.BCrypt.HashPassword("1234"), Profile.ADMIN);
        mockUserService.Setup(u => u.GetUserByEmail("admin@mail.com")).Returns(user);

        var auth = new AuthDto("admin@mail.com", "1234");
        var token = authService.UserLoginWeb(auth);

        var refreshed = authService.RefreshToken(token);

        Assert.NotNull(refreshed);
        Assert.Contains(".", refreshed);
        Assert.NotEqual(token, refreshed);
    }

    /// <summary>
    /// Tests refreshing an invalid JWT token.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenTokenInvalid()
    {
        var refreshed = authService.RefreshToken("invalid.token.string");

        Assert.Equal(string.Empty, refreshed);
    }

    /// <summary>
    ///  Tests successful web login for a user with the MANAGER role.
    /// </summary>
    [Fact]
    public void UserLoginWeb_Manager_ShouldReturnToken()
    {
        var user = new User(10, "Manager", "manager@mail.com", BCrypt.Net.BCrypt.HashPassword("1234"), Profile.MANAGER);
        mockUserService.Setup(u => u.GetUserByEmail("manager@mail.com")).Returns(user);

        var token = authService.UserLoginWeb(new AuthDto("manager@mail.com", "1234"));

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    /// <summary>
    /// Tests mobile login for a user with a missing email.
    /// </summary>
    [Fact]
    public void UserLoginMobile_ShouldThrowAuth_WhenEmailNotFound()
    {
        mockUserService.Setup(u => u.GetUserByEmail("missing@mail.com")).Returns((User?)null);

        Assert.Throws<AuthException>(() => authService.UserLoginWeb(new AuthDto("missing@mail.com", "123")));
    }

    [Fact]
    public void UserLoginWeb_ShouldThrowArgument_WhenEmailOrPasswordEmpty()
    {
        Assert.Throws<ArgumentException>(() => authService.UserLoginWeb(new AuthDto("", "123")));
        Assert.Throws<ArgumentException>(() => authService.UserLoginWeb(new AuthDto("a@mail.com", "")));
    }

    /// <summary>
    /// Tests web login for a user with a missing email.
    /// </summary>
    [Fact]
    public void UserLoginWeb_ShouldThrowAuth_WhenEmailNotFound()
    {
        mockUserService.Setup(u => u.GetUserByEmail("missing@mail.com")).Returns((User?)null);

        Assert.Throws<AuthException>(() =>
            authService.UserLoginWeb(new AuthDto("missing@mail.com", "123")));
    }


    /// <summary>
    /// Tests refreshing an expired JWT token.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenExpiredToken()
    {
        var token = CreateCustomJwt(new Dictionary<string, string>
        {
            [JwtRegisteredClaimNames.Sub] = "11",
            [JwtRegisteredClaimNames.Email] = "admin2@mail.com",
            [ClaimTypes.Role] = "ADMIN"
        }, expiresDays: -1);

        var refreshed = authService.RefreshToken(token);

        Assert.Equal(string.Empty, refreshed);
    }

    /// <summary>
    /// Tests refreshing a JWT token with a non-integer 'sub' claim.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenSubIsNotInt()
    {
        var token = CreateCustomJwt(new Dictionary<string, string>
        {
            [JwtRegisteredClaimNames.Sub] = "abc",
            [JwtRegisteredClaimNames.Email] = "a@mail.com",
            [ClaimTypes.Role] = "ADMIN"
        });

        var result = authService.RefreshToken(token);
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests refreshing a JWT token with a missing email claim.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenEmailMissing()
    {
        var token = CreateCustomJwt(new Dictionary<string, string>
        {
            [JwtRegisteredClaimNames.Sub] = "1",
            [ClaimTypes.Role] = "ADMIN"
        });

        var result = authService.RefreshToken(token);
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests refreshing a JWT token with an invalid role claim.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenRoleInvalid()
    {
        var token = CreateCustomJwt(new Dictionary<string, string>
        {
            [JwtRegisteredClaimNames.Sub] = "1",
            [JwtRegisteredClaimNames.Email] = "a@mail.com",
            [ClaimTypes.Role] = "INVALID_ROLE"
        });

        var result = authService.RefreshToken(token);
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests refreshing a JWT token with a whitespace string.
    /// </summary>
    [Fact]
    public void RefreshToken_ShouldReturnEmpty_WhenTokenWhitespace()
    {
        var result = authService.RefreshToken("   ");
        Assert.Equal(string.Empty, result);
    }
}

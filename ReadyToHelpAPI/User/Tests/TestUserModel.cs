using readytohelpapi.User.Models;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///   Unit tests for the User model (constructors and properties).
/// </summary>
[Trait("Category", "Unit")]
public class TestUserModel
{
    /// <summary>
    ///   Ensures default constructor sets expected defaults.
    /// </summary>
    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        var u = new Models.User
        {
            Name = string.Empty,
            Email = string.Empty,
            Password = string.Empty,
        };

        Assert.Equal(0, u.Id);
        Assert.Equal(string.Empty, u.Name);
        Assert.Equal(string.Empty, u.Email);
        Assert.Equal(string.Empty, u.Password);

        Assert.True(System.Enum.IsDefined(typeof(Profile), u.Profile));
    }

    /// <summary>
    ///   Ensures parameterized constructor sets all properties.
    /// </summary>
    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var u = new Models.User
        {
            Id = 5,
            Name = "Alice",
            Email = "alice@example.com",
            Password = "secret",
            Profile = Profile.ADMIN,
        };

        Assert.Equal(5, u.Id);
        Assert.Equal("Alice", u.Name);
        Assert.Equal("alice@example.com", u.Email);
        Assert.Equal("secret", u.Password);
        Assert.Equal(Profile.ADMIN, u.Profile);
    }

    /// <summary>
    ///   Ensures setters update properties correctly.
    /// </summary>
    [Fact]
    public void PropertySetters_UpdateValues()
    {
        var u = new Models.User
        {
            Name = "Bob",
            Email = "bob@example.com",
            Password = "pwd",
        };
        u.Id = 10;
        u.Profile = Profile.CITIZEN;

        Assert.Equal(10, u.Id);
        Assert.Equal("Bob", u.Name);
        Assert.Equal("bob@example.com", u.Email);
        Assert.Equal("pwd", u.Password);
        Assert.Equal(Profile.CITIZEN, u.Profile);
    }
}

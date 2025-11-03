using readytohelpapi.User.Models;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///   Unit tests for the User model (construtores e propriedades).
/// </summary>
[Trait("Category", "Unit")]
public class TestUserModel
{
    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        var u = new Models.User();

        Assert.Equal(0, u.Id);
        Assert.Null(u.Name);
        Assert.Null(u.Email);
        Assert.Null(u.Password);

        Assert.True(System.Enum.IsDefined(typeof(Profile), u.Profile));
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var u = new Models.User(
            id: 5,
            name: "Alice",
            email: "alice@example.com",
            password: "secret",
            profile: Profile.ADMIN
        );

        Assert.Equal(5, u.Id);
        Assert.Equal("Alice", u.Name);
        Assert.Equal("alice@example.com", u.Email);
        Assert.Equal("secret", u.Password);
        Assert.Equal(Profile.ADMIN, u.Profile);
    }

    /// <summary>
    ///   Verifica que os setters atualizam as propriedades corretamente.
    /// </summary>
    [Fact]
    public void PropertySetters_UpdateValues()
    {
        var u = new Models.User();
        u.Id = 10;
        u.Name = "Bob";
        u.Email = "bob@example.com";
        u.Password = "pwd";
        u.Profile = Profile.CITIZEN;

        Assert.Equal(10, u.Id);
        Assert.Equal("Bob", u.Name);
        Assert.Equal("bob@example.com", u.Email);
        Assert.Equal("pwd", u.Password);
        Assert.Equal(Profile.CITIZEN, u.Profile);
    }
}
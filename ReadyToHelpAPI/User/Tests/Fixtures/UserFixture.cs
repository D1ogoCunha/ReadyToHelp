namespace readytohelpapi.User.Tests.Fixtures;

using readytohelpapi.User.Models;

/// <summary>
///     Helper class to create or update User objects with default values for tests.
/// </summary>
public static class UserFixture
{
    /// <summary>
    ///     Creates or updates a User object with the specified values.
    /// </summary>
    public static User CreateOrUpdateUser(
        User? testUser = null,
        int? id = null,
        string name = "Default User",
        string email = "default@user.com",
        string password = "DefaultPassword",
        Profile profile = Profile.CITIZEN
    )
    {
        testUser ??= new User
        {
            Name = name,
            Email = email,
            Password = password,
        };

        if (id.HasValue)
            testUser.Id = id.Value;
        testUser.Name = name;
        testUser.Email = email;
        testUser.Password = password;
        testUser.Profile = profile;

        return testUser;
    }
}

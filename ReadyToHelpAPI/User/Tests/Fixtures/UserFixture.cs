namespace readytohelpapi.User.Tests.Fixtures;

/// <summary>
///     Helper class to create or update User objects with default values for tests.
/// </summary>
public static class UserFixture
{
    /// <summary>
    ///     Creates or updates a User object with the specified values.
    /// </summary>
    public static Models.User CreateOrUpdateUser(
        Models.User? testUser = null,
        int? id = null,
        string name = "Default User",
        string email = "default@user.com",
        string password = "DefaultPassword",
        Models.Profile profile = Models.Profile.CITIZEN)
    {
        testUser ??= new Models.User();

        if (id.HasValue) testUser.Id = id.Value;
        testUser.Name = name;
        testUser.Email = email;
        testUser.Password = password;
        testUser.Profile = profile;

        return testUser;
    }
}
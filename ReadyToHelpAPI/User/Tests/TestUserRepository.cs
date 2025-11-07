namespace readytohelpapi.User.Tests;

using readytohelpapi.Common.Data;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using Xunit;

/// <summary>
///  This class contains all integration tests related to the user repository.
/// </summary>
[Trait("Category", "Integration")]
public class TestUserRepositoryTest : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly AppDbContext userContext;
    private readonly IUserRepository _userRepository;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestUserRepositoryTest"/> class.
    /// </summary>
    public TestUserRepositoryTest(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();
        userContext = this.fixture.Context;
        _userRepository = new UserRepository(userContext);
    }

    /// <summary>
    /// Tests if a user can be retrieved by ID when the user exists.
    /// </summary>
    [Fact]
    public void GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = new User
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        var result = _userRepository.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by ID that does not exist.
    /// </summary>
    [Fact]
    public void GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var result = this._userRepository.GetUserById(999);
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if a user can be retrieved by email when the full email matches.
    /// </summary>
    [Fact]
    public void GetUserByEmail_ShouldReturnUser_WhenFullEmailMatches()
    {
        var user = new User
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        var result = _userRepository.GetUserByEmail("alice@example.com");

        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by email that does not exist.
    /// </summary>
    [Fact]
    public void GetUserByEmail_ShouldReturnNull_WhenNoMatch()
    {
        var result = _userRepository.GetUserByEmail("noone@example.com");
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by ID with invalid input (zero).
    /// </summary>
    [Fact]
    public void GetUserById_WithZeroId_ReturnsNull()
    {
        var result = _userRepository.GetUserById(0);
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by ID with invalid input (negative).
    /// </summary>
    [Fact]
    public void GetUserById_WithNegativeId_ReturnsNull()
    {
        var result = _userRepository.GetUserById(-5);
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by email with invalid input (null).
    /// </summary>
    [Fact]
    public void GetUserByEmail_WithNull_ReturnsNull()
    {
        var result = _userRepository.GetUserByEmail(null!);
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by email with invalid input (empty string).
    /// </summary>
    [Fact]
    public void GetUserByEmail_WithEmptyString_ReturnsNull()
    {
        var result = _userRepository.GetUserByEmail("");
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if null is returned when trying to retrieve a user by email with invalid input (whitespace).
    /// </summary>
    [Fact]
    public void GetUserByEmail_WithWhitespace_ReturnsNull()
    {
        var result = _userRepository.GetUserByEmail("   ");
        Assert.Null(result);
    }

    /// <summary>
    /// Tests if a user can be retrieved by email in a case-insensitive manner.
    /// </summary>
    [Fact]
    public void GetUserByEmail_CaseInsensitive_ReturnsUser()
    {
        var user = new User
        {
            Name = "Case Test",
            Email = "CaseSensitive@Example.COM",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        var result = _userRepository.GetUserByEmail("casesensitive@example.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    /// <summary>
    /// Tests if an empty list is returned when trying to retrieve users by name with invalid input (null).
    /// </summary>
    [Fact]
    public void GetUserByName_WithNull_ReturnsEmptyList()
    {
        var result = _userRepository.GetUserByName(null!);
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests if an empty list is returned when trying to retrieve users by name with invalid input (empty string).
    /// </summary>
    [Fact]
    public void GetUserByName_WithEmptyString_ReturnsEmptyList()
    {
        var result = _userRepository.GetUserByName("");
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests if an empty list is returned when trying to retrieve users by name with invalid input (whitespace).
    /// </summary>
    [Fact]
    public void GetUserByName_WithWhitespace_ReturnsEmptyList()
    {
        var result = _userRepository.GetUserByName("   ");
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests if users can be retrieved by name when there are partial matches.
    /// </summary>
    [Fact]
    public void GetUserByName_ShouldReturnList_WhenNamePartialMatches()
    {
        var u1 = new User
        {
            Name = "Johnny Appleseed",
            Email = "j1@example.com",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        var u2 = new User
        {
            Name = "John Smith",
            Email = "j2@example.com",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.AddRange(u1, u2);
        userContext.SaveChanges();

        var result = _userRepository.GetUserByName("John");

        Assert.NotEmpty(result);
        Assert.Contains(result, u => u.Name.Contains("John", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tests if all users can be retrieved when no filters are applied.
    /// </summary>
    [Fact]
    public void GetAllUsers_ShouldReturnFilteredUsers_WhenFilteredByName()
    {
        var user = new User
        {
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        var result = _userRepository.GetAllUsers(1, 10, "Name", "asc", "jane");

        Assert.Single(result);
        Assert.Equal("Jane Smith", result[0].Name);
    }

    /// <summary>
    /// Tests if users are returned sorted by name in descending order.
    /// </summary>
    [Fact]
    public void GetAllUsers_ShouldReturnSortedUsers_ByNameDesc()
    {
        var a = new User
        {
            Name = "Alpha",
            Email = "a@example.com",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        var z = new User
        {
            Name = "Zulu",
            Email = "z@example.com",
            Password = "p",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.AddRange(a, z);
        userContext.SaveChanges();

        var result = _userRepository.GetAllUsers(1, 10, "Name", "desc", string.Empty);

        Assert.True(result.Count >= 2);
        Assert.Equal("Zulu", result[0].Name);
    }

    /// <summary>
    /// Tests if users can be filtered by email.
    /// </summary>
    [Fact]
    public void GetAllUsers_FilterByEmail_ReturnsMatchingUsers()
    {
        var user1 = new User
        {
            Name = "User One",
            Email = "specific@domain.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        var user2 = new User
        {
            Name = "User Two",
            Email = "other@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.AddRange(user1, user2);
        userContext.SaveChanges();

        var result = _userRepository.GetAllUsers(1, 10, "Name", "asc", "specific@domain");

        Assert.Single(result);
        Assert.Equal("specific@domain.com", result[0].Email);
    }

    /// <summary>
    /// Tests if a valid user can be created successfully.
    /// </summary>
    [Fact]
    public void Create_ValidUser_ReturnsCreatedUser()
    {
        var newUser = new User
        {
            Name = "Bob",
            Email = "bob@example.com",
            Password = "secret",
            Profile = Profile.CITIZEN,
        };

        var created = _userRepository.Create(newUser);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("bob@example.com", created.Email);
    }

    /// <summary>
    /// Tests if creating a null user throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Create_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _userRepository.Create(null!));
    }

    /// <summary>
    /// Tests if an existing user can be updated successfully.
    /// </summary>
    [Fact]
    public void Update_ExistingUser_ReturnsUpdatedUser()
    {
        var user = new User
        {
            Name = "UpdateMe",
            Email = "up@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        user.Email = "updated@example.com";
        var updated = _userRepository.Update(user);

        Assert.NotNull(updated);
        Assert.Equal("updated@example.com", updated.Email);
    }

    /// <summary>
    /// Tests if updating a non-existing user throws a DbUpdateException.
    /// </summary>
    [Fact]
    public void Update_NonExistingUser_ThrowsDbUpdateException()
    {
        var user = new User
        {
            Id = 99999,
            Name = "Ghost User",
            Email = "ghost@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };

        Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(() =>
            _userRepository.Update(user)
        );
    }

    /// <summary>
    /// Tests if updating a null user throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Update_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _userRepository.Update(null!));
    }

    /// <summary>
    /// Tests if multiple fields of a user can be updated successfully.
    /// </summary>
    [Fact]
    public void Update_ChangesMultipleFields_SavesCorrectly()
    {
        var user = new User
        {
            Name = "Original Name",
            Email = "original@example.com",
            Password = "oldpwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        user.Name = "Updated Name";
        user.Email = "updated@example.com";
        user.Password = "newpwd";
        user.Profile = Profile.ADMIN;

        var updated = _userRepository.Update(user);

        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("updated@example.com", updated.Email);
        Assert.Equal("newpwd", updated.Password);
        Assert.Equal(Profile.ADMIN, updated.Profile);
    }

    /// <summary>
    /// Tests if an existing user can be deleted successfully.
    /// </summary>
    [Fact]
    public void Delete_ExistingUser_ReturnsDeletedUser()
    {
        var user = new User
        {
            Name = "ToDelete",
            Email = "del@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN,
        };
        userContext.Users.Add(user);
        userContext.SaveChanges();

        var deleted = _userRepository.Delete(user.Id);

        Assert.NotNull(deleted);
        Assert.Equal(user.Id, deleted.Id);

        var after = _userRepository.GetUserById(user.Id);
        Assert.Null(after);
    }

    /// <summary>
    /// Tests if deleting a non-existing user returns null.
    /// </summary>
    [Fact]
    public void Delete_NonExisting_ReturnsNull()
    {
        var result = _userRepository.Delete(999999);
        Assert.Null(result);
    }
}

namespace readytohelpapi.User.Tests;

using Moq;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using Xunit;

/// <summary>
///  This class contains all unit tests related to the user service.
/// </summary>
public class TestUserServiceTest
{
    private readonly Mock<IUserRepository> mockRepo;
    private readonly IUserService userService;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestUserServiceTest"/> class.
    /// </summary>
    public TestUserServiceTest()
    {
        mockRepo = new Mock<IUserRepository>();
        userService = new UserServiceImpl(mockRepo.Object);
    }

    /// <summary>
    ///   Tests the Create method with a null user.
    /// </summary>
    [Fact]
    public void Create_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => userService.Create(null));
    }

    /// <summary>
    ///   Tests the Create method with an empty name.
    /// </summary>
    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        var user = new Models.User(0, "", "email@example.com", "password", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Create(user));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Create method with an empty password.
    /// </summary>
    [Fact]
    public void Create_EmptyPassword_ThrowsArgumentException()
    {
        var user = new Models.User(0, "Name", "email@example.com", "", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Create(user));
        Assert.Contains("password", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Create method with an invalid profile.
    /// </summary>
    [Fact]
    public void Create_InvalidProfile_ThrowsArgumentOutOfRangeException()
    {
        var user = new Models.User(0, "Name", "email@example.com", "password", (Profile)999);
        Assert.Throws<ArgumentOutOfRangeException>(() => userService.Create(user));
    }

    /// <summary>
    ///   Tests the Create method with an empty email.
    /// </summary>
    [Fact]
    public void Create_EmptyEmail_ThrowsArgumentException()
    {
        var user = new Models.User(0, "Name", "", "password", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Create(user));
        Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Create method when the email already exists.
    /// </summary>
    [Fact]
    public void Create_EmailAlreadyExists_ThrowsArgumentException()
    {
        var user = new Models.User(
            0,
            "Name",
            "exists@example.com",
            "password",
            Profile.CITIZEN
        );
        mockRepo
            .Setup(r => r.GetUserByEmail(It.IsAny<string>()))
            .Returns(new Models.User(1, "Other", "exists@example.com", "pwd", Profile.CITIZEN));

        var ex = Assert.Throws<ArgumentException>(() => userService.Create(user));
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Create method when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Create_RepositoryThrows_Exception()
    {
        var user = new Models.User(0, "Name", "new@example.com", "password", Profile.CITIZEN);
        mockRepo.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns((Models.User?)null);
        mockRepo
            .Setup(r => r.Create(It.IsAny<Models.User>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => userService.Create(user));
    }

    /// <summary>
    ///   Tests the Create method with valid input.
    /// </summary>
    [Fact]
    public void Create_Valid_ReturnsCreatedUser()
    {
        var user = new Models.User(
            0,
            "Alice",
            "alice@example.com",
            "password",
            Profile.CITIZEN
        );
        var created = new Models.User(
            10,
            "Alice",
            "alice@example.com",
            "hashed",
            Profile.CITIZEN
        );

        mockRepo.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns((Models.User?)null);
        mockRepo.Setup(r => r.Create(It.IsAny<Models.User>())).Returns(created);

        var result = userService.Create(user);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    /// <summary>
    ///   Tests the Update method with a null user.
    /// </summary>
    [Fact]
    public void Update_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => userService.Update(null));
    }

    /// <summary>
    ///   Tests the Update method with an invalid user ID.
    /// </summary>
    [Fact]
    public void Update_InvalidId_ThrowsArgumentException()
    {
        var user = new Models.User(0, "Name", "n@example.com", "password", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Update(user));
        Assert.Contains("Invalid user id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Update method with an empty name.
    /// </summary>
    [Fact]
    public void Update_EmptyName_ThrowsArgumentException()
    {
        var user = new Models.User(5, "", "email@example.com", "password", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Update(user));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Update method when the email belongs to another user.
    /// </summary>
    [Fact]
    public void Update_EmailBelongsToOtherUser_ThrowsArgumentException()
    {
        var existing = new Models.User(
            5,
            "User5",
            "user5@example.com",
            "hashed",
            Profile.CITIZEN
        );
        var other = new Models.User(
            6,
            "User6",
            "conflict@example.com",
            "hashed",
            Profile.CITIZEN
        );
        var toUpdate = new Models.User(
            5,
            "User5",
            "conflict@example.com",
            "password",
            Profile.CITIZEN
        );

        mockRepo.Setup(r => r.GetUserById(5)).Returns(existing);
        mockRepo.Setup(r => r.GetUserByEmail("conflict@example.com")).Returns(other);

        var ex = Assert.Throws<ArgumentException>(() => userService.Update(toUpdate));
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Update method with a null password, ensuring the existing password is kept.
    /// </summary>
    [Fact]
    public void Update_NullPassword_KeepsExistingPassword()
    {
        var existing = new Models.User(
            5,
            "Old",
            "old@example.com",
            "existingHash",
            Profile.CITIZEN
        );
        var toUpdate = new Models.User(5, "Updated", "old@example.com", null, Profile.CITIZEN);

        mockRepo.Setup(r => r.GetUserById(5)).Returns(existing);
        mockRepo.Setup(r => r.GetUserByEmail("old@example.com")).Returns(existing);
        mockRepo
            .Setup(r => r.Update(It.Is<Models.User>(u => u.Password == "existingHash")))
            .Returns(toUpdate);

        var result = userService.Update(toUpdate);
        Assert.NotNull(result);
    }

    /// <summary>
    ///   Tests the Update method with an empty email.
    /// </summary>
    [Fact]
    public void Update_EmptyEmail_ThrowsArgumentException()
    {
        var user = new Models.User(5, "Name", "", "password", Profile.CITIZEN);
        var ex = Assert.Throws<ArgumentException>(() => userService.Update(user));
        Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Tests the Update method when the user is not found.
    /// </summary>
    [Fact]
    public void Update_UserNotFound_ThrowsKeyNotFoundException()
    {
        var user = new Models.User(5, "Name", "n@example.com", "password", Profile.CITIZEN);
        mockRepo.Setup(r => r.GetUserById(user.Id)).Returns((Models.User?)null);

        Assert.Throws<KeyNotFoundException>(() => userService.Update(user));
    }

    /// <summary>
    ///   Tests the Update method when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Update_RepositoryThrows_ExceptionPropagated()
    {
        var existing = new Models.User(5, "Old", "old@example.com", "hashed", Profile.CITIZEN);
        var toUpdate = new Models.User(
            5,
            "New",
            "new@example.com",
            "password",
            Profile.CITIZEN
        );

        mockRepo.Setup(r => r.GetUserById(existing.Id)).Returns(existing);
        mockRepo.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns((Models.User?)null);
        mockRepo
            .Setup(r => r.Update(It.IsAny<Models.User>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() => userService.Update(toUpdate));
    }

    /// <summary>
    ///   Tests the Update method with valid input.
    /// </summary>
    [Fact]
    public void Update_Valid_ReturnsUpdatedUser()
    {
        var existing = new Models.User(6, "Old", "old@example.com", "hashed", Profile.CITIZEN);
        var toUpdate = new Models.User(
            6,
            "Updated",
            "old@example.com",
            "password",
            Profile.CITIZEN
        );

        mockRepo.Setup(r => r.GetUserById(existing.Id)).Returns(existing);
        mockRepo.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns(existing);
        mockRepo.Setup(r => r.Update(It.IsAny<Models.User>())).Returns(toUpdate);

        var result = userService.Update(toUpdate);

        Assert.NotNull(result);
        Assert.Equal(toUpdate.Id, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    /// <summary>
    ///   Tests the Delete method with an invalid user ID.
    /// </summary>
    [Fact]
    public void Delete_InvalidId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => userService.Delete(0));
    }

    /// <summary>
    ///   Tests the Delete method with a valid user ID.
    /// </summary>
    [Fact]
    public void Delete_ShouldReturnDeletedUser_WhenUserIdIsValid()
    {
        var user = new Models.User(7, "ToDelete", "del@example.com", "pwd", Profile.CITIZEN);
        mockRepo.Setup(r => r.Delete(user.Id)).Returns(user);

        var result = userService.Delete(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    /// <summary>
    ///   Tests the Delete method when the repository throws an exception.
    /// </summary>
    [Fact]
    public void Delete_RepositoryThrows_ExceptionPropagated()
    {
        mockRepo.Setup(r => r.Delete(It.IsAny<int>())).Throws(new Exception("DB error"));
        Assert.Throws<InvalidOperationException>(() => userService.Delete(8));
    }

    /// <summary>
    ///  Tests the GetUserById method with an invalid user ID.
    /// </summary>
    [Fact]
    public void GetUserById_InvalidId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => userService.GetUserById(0));
        Assert.Contains("Invalid user id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetUserById method when the user is not found.
    /// </summary>
    [Fact]
    public void GetUserById_UserNotFound_ThrowsKeyNotFoundException()
    {
        mockRepo.Setup(r => r.GetUserById(999)).Returns((Models.User?)null);
        Assert.Throws<KeyNotFoundException>(() => userService.GetUserById(999));
    }

    /// <summary>
    ///  Tests the GetUserById method with a valid user ID.
    /// </summary>
    [Fact]
    public void GetUserById_Valid_ReturnsUser()
    {
        var user = new Models.User(10, "Test", "test@example.com", "hash", Profile.CITIZEN);
        mockRepo.Setup(r => r.GetUserById(10)).Returns(user);

        var result = userService.GetUserById(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
    }

    /// <summary>
    ///  Tests the GetUserByName method with a partial search term, ensuring it returns the matching users. 
    /// </summary>
    [Fact]
    public void GetUserByName_ReturnsMatchingUsers()
    {
        var users = new List<Models.User>
            {
                new Models.User(1, "Alice", "alice@example.com", "hash", Profile.CITIZEN),
                new Models.User(2, "Alicia", "alicia@example.com", "hash", Profile.ADMIN)
            };
        mockRepo.Setup(r => r.GetUserByName("Ali")).Returns(users);

        var result = userService.GetUserByName("Ali");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    ///  Tests the GetUserByEmail method with an empty email.
    /// </summary>
    [Fact]
    public void GetUserByEmail_EmptyEmail_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => userService.GetUserByEmail(""));
        Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetUserByEmail method with a valid email.
    /// </summary>
    [Fact]
    public void GetUserByEmail_Valid_ReturnsUser()
    {
        var user = new Models.User(1, "Test", "test@example.com", "hash", Profile.CITIZEN);
        mockRepo.Setup(r => r.GetUserByEmail("test@example.com")).Returns(user);

        var result = userService.GetUserByEmail("test@example.com");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    /// <summary>
    ///  Tests the GetUserByEmail method when the user is not found.
    /// </summary>
    [Fact]
    public void GetUserByEmail_NotFound_ReturnsNull()
    {
        mockRepo.Setup(r => r.GetUserByEmail("notfound@example.com")).Returns((Models.User?)null);

        var result = userService.GetUserByEmail("notfound@example.com");

        Assert.Null(result);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with invalid parameters.
    /// </summary>
    [Fact]
    public void GetAllUsers_InvalidPageNumber_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            userService.GetAllUsers(0, 10, "Name", "asc", ""));
        Assert.Contains("Page number", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with invalid page size.
    /// </summary>
    [Fact]
    public void GetAllUsers_InvalidPageSize_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            userService.GetAllUsers(1, 0, "Name", "asc", ""));
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with too large page size.
    /// </summary>
    [Fact]
    public void GetAllUsers_PageSizeTooLarge_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            userService.GetAllUsers(1, 1001, "Name", "asc", ""));
        Assert.Contains("Page size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with an empty sort by field.
    /// </summary>
    [Fact]
    public void GetAllUsers_EmptySortBy_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            userService.GetAllUsers(1, 10, "", "asc", ""));
        Assert.Contains("Sort field", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with an invalid sort order.
    /// </summary>
    [Fact]
    public void GetAllUsers_InvalidSortOrder_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            userService.GetAllUsers(1, 10, "Name", "invalid", ""));
        Assert.Contains("Sort order", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///  Tests the GetAllUsers method with valid parameters.
    /// </summary>
    [Fact]
    public void GetAllUsers_Valid_ReturnsUsers()
    {
        var users = new List<Models.User>
            {
                new Models.User(1, "Alice", "alice@example.com", "hash", Profile.CITIZEN)
            };
        mockRepo.Setup(r => r.GetAllUsers(1, 10, "Name", "asc", ""))
            .Returns(users);

        var result = userService.GetAllUsers(1, 10, "Name", "asc", "");

        Assert.NotNull(result);
        Assert.Single(result);
    }

    /// <summary>
    ///  Tests the GetAllUsers method when the repository returns null.
    /// </summary>
    [Fact]
    public void GetAllUsers_RepositoryReturnsNull_ReturnsEmptyList()
    {
        mockRepo.Setup(r => r.GetAllUsers(1, 10, "Name", "asc", ""))
            .Returns((List<Models.User>?)null);

        var result = userService.GetAllUsers(1, 10, "Name", "asc", "");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    ///  Tests the GetAllUsers method when the repository throws an exception.
    /// </summary>
    [Fact]
    public void GetAllUsers_RepositoryThrows_ExceptionPropagated()
    {
        mockRepo.Setup(r => r.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<InvalidOperationException>(() =>
            userService.GetAllUsers(1, 10, "Name", "asc", ""));
    }

    /// <summary>
    ///  Tests the Register method with a null user.
    /// </summary>
    [Fact]
    public void Register_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => userService.Register(null));
    }

    /// <summary>
    ///  Tests the Register method forcing the profile to CITIZEN.
    /// </summary>
    [Fact]
    public void Register_ForcesProfileToCitizen()
    {
        var user = new Models.User(0, "NewUser", "new@example.com", "password", Profile.ADMIN);
        var created = new Models.User(1, "NewUser", "new@example.com", "hash", Profile.CITIZEN);

        mockRepo.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns((Models.User?)null);
        mockRepo.Setup(r => r.Create(It.Is<Models.User>(u => u.Profile == Profile.CITIZEN)))
            .Returns(created);

        var result = userService.Register(user);

        Assert.NotNull(result);
        Assert.Equal(Profile.CITIZEN, result.Profile);
    }

}


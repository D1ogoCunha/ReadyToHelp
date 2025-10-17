using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.User.Controllers;
using readytohelpapi.User.Services;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///   This class contains all unit tests related to the user api controller.
/// </summary>
public class TestUserApiController : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly Mock<IUserService> mockUserService;
    private readonly UserApiController controller;

    /// <summary>
    ///   Initializes a new instance of the <see cref="TestUserApiController"/> class.
    /// </summary>
    public TestUserApiController(DbFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.ResetDatabase();

        mockUserService = new Mock<IUserService>();
        controller = new UserApiController(mockUserService.Object);
    }

    /// <summary>
    ///   Tests the Create method with a null user.
    /// </summary>
    [Fact]
    public void Create_NullUser_ReturnsBadRequest()
    {
        var result = controller.Create(null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Create method with a valid user.
    /// </summary>
    [Fact]
    public void Create_ValidUser_ReturnsCreatedAtAction()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 1, name: "Alice", email: "alice@example.com");
        mockUserService.Setup(s => s.Create(It.IsAny<Models.User>())).Returns(user);

        var result = controller.Create(user);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), created.ActionName);
    }

    /// <summary>
    ///   Tests the Create method when the email already exists.
    /// </summary>
    [Fact]
    public void Create_EmailAlreadyExists_ReturnsConflict()
    {
        var user = UserFixture.CreateOrUpdateUser(
            id: 0,
            name: "Existing",
            email: "exists@example.com"
        );
        mockUserService
            .Setup(s => s.Create(It.IsAny<Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Create(user);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Create method when the service throws a generic exception.
    ///   Expects an Internal Server Error (500).
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 0, name: "Test");
        mockUserService
            .Setup(s => s.Create(It.IsAny<Models.User>()))
            .Throws(new Exception("Unexpected error"));

        var result = controller.Create(user);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the Update method with a null user.
    /// </summary>
    [Fact]
    public void Update_NullUser_ReturnsBadRequest()
    {
        var result = controller.Update(1, null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Update method with a valid user.
    /// </summary>
    [Fact]
    public void Update_ValidUser_ReturnsOk()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 2, name: "Bob", email: "bob@example.com");
        mockUserService.Setup(s => s.Update(It.IsAny<Models.User>())).Returns(user);

        var result = controller.Update(2, user);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    /// <summary>
    ///   Tests the Update method when the IDs do not match.
    /// </summary>
    [Fact]
    public void Update_IdMismatch_UpdatesWithRouteId()
    {
        var user = UserFixture.CreateOrUpdateUser(
            id: 999,
            name: "Wrong",
            email: "wrong@example.com"
        );
        var updated = UserFixture.CreateOrUpdateUser(
            id: 10,
            name: "Wrong",
            email: "wrong@example.com"
        );
        mockUserService.Setup(s => s.Update(It.Is<Models.User>(u => u.Id == 10))).Returns(updated);

        var result = controller.Update(10, user);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<Models.User>(ok.Value);
        Assert.Equal(10, returned.Id);
    }

    /// <summary>
    ///  Tests the Update method when the service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsArgumentNullException_ReturnsBadRequest()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 5);
        mockUserService
            .Setup(s => s.Update(It.IsAny<Models.User>()))
            .Throws(new ArgumentNullException("user"));

        var result = controller.Update(5, user);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Update method when the email already exists.
    /// </summary>
    [Fact]
    public void Update_EmailAlreadyExists_ReturnsConflict()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 7, email: "conflict@example.com");
        mockUserService
            .Setup(s => s.Update(It.IsAny<Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Update(7, user);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///  Tests the Update method when the service throws a KeyNotFoundException.
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 999);
        mockUserService
            .Setup(s => s.Update(It.IsAny<Models.User>()))
            .Throws(new KeyNotFoundException("User not found"));

        var result = controller.Update(999, user);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the Update method when the service throws a generic exception.
    ///   Expects an Internal Server Error (500).
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 8);
        mockUserService
            .Setup(s => s.Update(It.IsAny<Models.User>()))
            .Throws(new Exception("DB error"));

        var result = controller.Update(8, user);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the Delete method with a non-existing user.
    /// </summary>
    [Fact]
    public void Delete_NonExisting_ReturnsNotFound()
    {
        mockUserService.Setup(s => s.Delete(It.IsAny<int>())).Returns((Models.User?)null);

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the Delete method with an existing user.
    /// </summary>
    [Fact]
    public void Delete_Existing_ReturnsOk()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 3, name: "Carol");
        mockUserService.Setup(s => s.Delete(user.Id)).Returns(user);

        var result = controller.Delete(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    /// <summary>
    ///   Tests the Delete method when the service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Delete_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockUserService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new ArgumentException("Invalid id"));

        var result = controller.Delete(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Delete method when the service throws a KeyNotFoundException.
    /// </summary>
    [Fact]
    public void Delete_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        mockUserService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new KeyNotFoundException("User not found"));

        var result = controller.Delete(888);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the Delete method when the service throws a generic exception.
    ///   Expects an Internal Server Error (500).
    /// </summary>
    [Fact]
    public void Delete_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockUserService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new Exception("DB connection error"));

        var result = controller.Delete(5);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the GetUserById method when the id is invalid.
    /// </summary>
    [Fact]
    public void GetUserById_InvalidId_ReturnsBadRequest()
    {
        mockUserService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new ArgumentException("Invalid user id"));

        var result = controller.GetUserById(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the GetUserById method when the user is not found.
    /// </summary>
    [Fact]
    public void GetUserById_UserNotFound_ReturnsNotFound()
    {
        mockUserService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new KeyNotFoundException("User not found"));

        var result = controller.GetUserById(777);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the GetUserById method when the service throws a generic exception.
    ///   Expects an Internal Server Error (500).
    /// </summary>
    [Fact]
    public void GetUserById_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockUserService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new Exception("DB error"));

        var result = controller.GetUserById(1);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the Register method when the request is null.
    /// </summary>
    [Fact]
    public void Register_NullRequest_ReturnsBadRequest()
    {
        var result = controller.Register(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Register method when the name is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyName_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("", "email@example.com", "password");

        var result = controller.Register(req);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Register method when the email is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyEmail_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("Name", "", "password");

        var result = controller.Register(req);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Register method when the password is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyPassword_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("Name", "email@example.com", "");

        var result = controller.Register(req);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Register method with a valid request.
    /// </summary>
    [Fact]
    public void Register_ValidRequest_ReturnsCreatedAtAction()
    {
        var req = new UserApiController.RegisterRequest(
            "NewUser",
            "new@example.com",
            "password123"
        );
        var created = UserFixture.CreateOrUpdateUser(
            id: 20,
            name: "NewUser",
            email: "new@example.com"
        );
        created.Profile = Models.Profile.CITIZEN;

        mockUserService.Setup(s => s.Register(It.IsAny<Models.User>())).Returns(created);

        var result = controller.Register(req);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), createdResult.ActionName);
    }

    /// <summary>
    ///   Tests the Register method when the email already exists.
    /// </summary>
    [Fact]
    public void Register_EmailAlreadyExists_ReturnsConflict()
    {
        var req = new UserApiController.RegisterRequest("User", "exists@example.com", "password");
        mockUserService
            .Setup(s => s.Register(It.IsAny<Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Register(req);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///   Tests the Register method when the service throws a generic exception.
    /// </summary>
    [Fact]
    public void Register_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var req = new UserApiController.RegisterRequest("User", "test@example.com", "password");
        mockUserService
            .Setup(s => s.Register(It.IsAny<Models.User>()))
            .Throws(new Exception("Unexpected error"));

        var result = controller.Register(req);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the GetAll method returning a list of users.
    /// </summary>
    [Fact]
    public void GetAll_ReturnsOkWithList()
    {
        var users = new List<Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 4, name: "U1"),
            UserFixture.CreateOrUpdateUser(id: 5, name: "U2"),
        };
        mockUserService
            .Setup(s =>
                s.GetAllUsers(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Returns(users);

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(users, ok.Value);
    }

    /// <summary>
    ///   Tests the GetAll method when the service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockUserService
            .Setup(s =>
                s.GetAllUsers(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Throws(new ArgumentException("Invalid"));

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var bad = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.NotNull(bad.Value);
    }

    /// <summary>
    ///   Tests the GetAll method with default parameters.
    /// </summary>
    [Fact]
    public void GetAll_WithDefaultParameters_ReturnsOk()
    {
        var users = new List<Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 30, name: "Default1"),
            UserFixture.CreateOrUpdateUser(id: 31, name: "Default2"),
        };
        mockUserService.Setup(s => s.GetAllUsers(1, 10, "Name", "asc", "")).Returns(users);

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(ok.Value);
    }

    /// <summary>
    ///   Tests the GetAll method with custom parameters.
    /// </summary>
    [Fact]
    public void GetAll_WithCustomParameters_ReturnsOk()
    {
        var users = new List<Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 32, name: "Custom"),
        };
        mockUserService.Setup(s => s.GetAllUsers(2, 5, "Email", "desc", "filter")).Returns(users);

        var result = controller.GetAll(2, 5, "Email", "desc", "filter");

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(ok.Value);
    }

    /// <summary>
    ///   Tests the GetAll method when the service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockUserService
            .Setup(s =>
                s.GetAllUsers(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Throws(new Exception("Database error"));

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var status = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the GetUserByEmail method when the user is not found.
    /// </summary>
    [Fact]
    public void GetUserByEmail_NotFound_ReturnsNotFound()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Returns((Models.User?)null);

        var result = controller.GetUserByEmail("noone@example.com");

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the GetUserByEmail method when the user is found.
    /// </summary>
    [Fact]
    public void GetUserByEmail_Found_ReturnsOk()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 6, name: "Found", email: "found@example.com");
        mockUserService.Setup(s => s.GetUserByEmail("found@example.com")).Returns(user);

        var result = controller.GetUserByEmail("found@example.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    /// <summary>
    ///   Tests the GetUserByEmail method when the service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void GetUserByEmail_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Throws(new ArgumentException("Invalid email"));

        var result = controller.GetUserByEmail("");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Tests the GetUserByEmail method when the service throws a generic exception.
    /// </summary>
    [Fact]
    public void GetUserByEmail_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Throws(new Exception("Database error"));

        var result = controller.GetUserByEmail("test@example.com");

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Tests the GetUserByEmail method with a whitespace email.
    /// </summary>
    [Fact]
    public void GetUserByEmail_WithWhitespaceEmail_ReturnsNotFoundOrThrows()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Returns((Models.User?)null);

        var result = controller.GetUserByEmail("   ");

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Tests the GetUserById method with a valid id.
    /// </summary>
    [Fact]
    public void GetUserById_ValidId_ReturnsUser()
    {
        fixture.ResetDatabase();
        var user = new Models.User
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password",
            Profile = Models.Profile.CITIZEN,
        };
        fixture.Context.Users.Add(user);
        fixture.Context.SaveChanges();

        mockUserService.Setup(s => s.GetUserById(user.Id)).Returns(user);

        var result = controller.GetUserById(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<Models.User>(ok.Value);
        Assert.Equal(user.Id, returned.Id);
        Assert.Equal(user.Email, returned.Email);
        Assert.Equal(user.Name, returned.Name);
    }

    /// <summary>
    ///   Tests the GetUsers method when users are found with pagination.
    /// </summary>
    [Fact]
    public void GetUsers_UsersFound_ReturnsOkWithPagination()
    {
        fixture.ResetDatabase();
        var u1 = new Models.User
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password",
            Profile = Models.Profile.CITIZEN,
        };
        var u2 = new Models.User
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Password = "password",
            Profile = Models.Profile.CITIZEN,
        };
        fixture.Context.Users.AddRange(u1, u2);
        fixture.Context.SaveChanges();

        var users = new List<Models.User> { u1, u2 };
        mockUserService
            .Setup(s =>
                s.GetAllUsers(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Returns(users);

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<Models.User>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Models.User>>(ok.Value);
        var list = returned.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Email == u1.Email && x.Name == u1.Name);
        Assert.Contains(list, x => x.Email == u2.Email && x.Name == u2.Name);
    }
}

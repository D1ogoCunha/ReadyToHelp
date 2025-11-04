using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.User.Controllers;
using readytohelpapi.User.DTOs;
using readytohelpapi.User.Services;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///   Unit tests for UserApiController.
/// </summary>
[Trait("Category", "Unit")]
public class TestUserApiController_Unit
{
    private readonly Mock<IUserService> mockUserService;
    private readonly UserApiController controller;

    /// <summary>
    ///   Initializes controller with mocked IUserService.
    /// </summary>
    public TestUserApiController_Unit()
    {
        mockUserService = new Mock<IUserService>();
        controller = new UserApiController(mockUserService.Object);
    }

    /// <summary>
    ///   Returns BadRequest when body is null.
    /// </summary>
    [Fact]
    public void Create_NullUser_ReturnsBadRequest()
    {
        var result = controller.Create(null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns CreatedAtAction with DTO on valid create.
    /// </summary>
    [Fact]
    public void Create_ValidUser_ReturnsCreatedAtAction()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 1, name: "Alice", email: "alice@example.com");
        mockUserService.Setup(s => s.Create(It.IsAny<readytohelpapi.User.Models.User>())).Returns(user);

        var result = controller.Create(user);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), created.ActionName);
        var dto = Assert.IsType<UserResponseDto>(created.Value);
        Assert.Equal(user.Id, dto.Id);
    }

    /// <summary>
    ///   Returns Conflict when email already exists.
    /// </summary>
    [Fact]
    public void Create_EmailAlreadyExists_ReturnsConflict()
    {
        var user = UserFixture.CreateOrUpdateUser(email: "exists@example.com");
        mockUserService
            .Setup(s => s.Create(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Create(user);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///   Returns 500 on unexpected exception.
    /// </summary>
    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var user = UserFixture.CreateOrUpdateUser(name: "Test");
        mockUserService
            .Setup(s => s.Create(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new Exception("Unexpected error"));

        var result = controller.Create(user);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Returns BadRequest when body is null.
    /// </summary>
    [Fact]
    public void Update_NullUser_ReturnsBadRequest()
    {
        var result = controller.Update(1, null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns Ok with DTO on valid update.
    /// </summary>
    [Fact]
    public void Update_ValidUser_ReturnsOk_WithDto()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 2, name: "Bob", email: "bob@example.com");
        mockUserService.Setup(s => s.Update(It.IsAny<readytohelpapi.User.Models.User>())).Returns(user);

        var result = controller.Update(2, user);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal(user.Name, dto.Name);
    }

    /// <summary>
    ///   Uses route id and returns DTO when body id mismatches.
    /// </summary>
    [Fact]
    public void Update_IdMismatch_UpdatesWithRouteId_ReturnsDto()
    {
        var body = UserFixture.CreateOrUpdateUser(id: 999, name: "Wrong", email: "wrong@example.com");
        var updated = UserFixture.CreateOrUpdateUser(id: 10, name: "Wrong", email: "wrong@example.com");

        mockUserService.Setup(s => s.Update(It.Is<readytohelpapi.User.Models.User>(u => u.Id == 10))).Returns(updated);

        var result = controller.Update(10, body);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(10, returned.Id);
    }

    /// <summary>
    ///   Returns BadRequest when service throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsArgumentNullException_ReturnsBadRequest()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 5);
        mockUserService
            .Setup(s => s.Update(It.IsAny<Models.User>()))
            .Throws<ArgumentNullException>();

        var result = controller.Update(5, user);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns Conflict when email already exists.
    /// </summary>
    [Fact]
    public void Update_EmailAlreadyExists_ReturnsConflict()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 7, email: "conflict@example.com");
        mockUserService
            .Setup(s => s.Update(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Update(7, user);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///   Returns NotFound when user is not found.
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 999);
        mockUserService
            .Setup(s => s.Update(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new KeyNotFoundException("User not found"));

        var result = controller.Update(999, user);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Returns 500 on unexpected exception.
    /// </summary>
    [Fact]
    public void Update_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 8);
        mockUserService
            .Setup(s => s.Update(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new Exception("DB error"));

        var result = controller.Update(8, user);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Returns NotFound when delete returns null.
    /// </summary>
    [Fact]
    public void Delete_NonExisting_ReturnsNotFound()
    {
        mockUserService.Setup(s => s.Delete(It.IsAny<int>())).Returns((Models.User?)null!);


        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Returns Ok with DTO on successful delete.
    /// </summary>
    [Fact]
    public void Delete_Existing_ReturnsOk_WithDto()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 3, name: "Carol");
        mockUserService.Setup(s => s.Delete(user.Id)).Returns(user);

        var result = controller.Delete(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(user.Id, dto.Id);
    }

    /// <summary>
    ///   Returns BadRequest on invalid id.
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
    ///   Returns NotFound when user is not found.
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
    ///   Returns 500 on unexpected exception.
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
    ///   Returns BadRequest on invalid id.
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
    ///   Returns NotFound when user is not found.
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
    ///   Returns 500 on unexpected exception.
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
    ///   Returns BadRequest when body is null.
    /// </summary>
    [Fact]
    public void Register_NullRequest_ReturnsBadRequest()
    {
        var result = controller.Register(null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns BadRequest when name is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyName_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("", "email@example.com", "password");
        var result = controller.Register(req);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns BadRequest when email is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyEmail_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("Name", "", "password");
        var result = controller.Register(req);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns BadRequest when password is empty.
    /// </summary>
    [Fact]
    public void Register_EmptyPassword_ReturnsBadRequest()
    {
        var req = new UserApiController.RegisterRequest("Name", "email@example.com", "");
        var result = controller.Register(req);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    ///   Returns CreatedAtAction with DTO on valid register.
    /// </summary>
    [Fact]
    public void Register_ValidRequest_ReturnsCreatedAtAction_WithDto()
    {
        var req = new UserApiController.RegisterRequest("NewUser", "new@example.com", "password123");
        var created = UserFixture.CreateOrUpdateUser(id: 20, name: "NewUser", email: "new@example.com");
        created.Profile = readytohelpapi.User.Models.Profile.CITIZEN;

        mockUserService.Setup(s => s.Register(It.IsAny<readytohelpapi.User.Models.User>())).Returns(created);

        var result = controller.Register(req);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), createdResult.ActionName);
        var dto = Assert.IsType<UserResponseDto>(createdResult.Value);
        Assert.Equal(created.Id, dto.Id);
    }

    /// <summary>
    ///   Returns Conflict when email exists on register.
    /// </summary>
    [Fact]
    public void Register_EmailAlreadyExists_ReturnsConflict()
    {
        var req = new UserApiController.RegisterRequest("User", "exists@example.com", "password");
        mockUserService
            .Setup(s => s.Register(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Register(req);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    ///   Returns 500 on unexpected exception during register.
    /// </summary>
    [Fact]
    public void Register_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        var req = new UserApiController.RegisterRequest("User", "test@example.com", "password");
        mockUserService
            .Setup(s => s.Register(It.IsAny<readytohelpapi.User.Models.User>()))
            .Throws(new Exception("Unexpected error"));

        var result = controller.Register(req);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Returns Ok with list of DTOs.
    /// </summary>
    [Fact]
    public void GetAll_ReturnsOkWithList_OfDto()
    {
        var users = new List<readytohelpapi.User.Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 4, name: "U1", email: "u1@x.com"),
            UserFixture.CreateOrUpdateUser(id: 5, name: "U2", email: "u2@x.com"),
        };
        mockUserService
            .Setup(s => s.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(users);

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var list = Assert.IsType<List<UserResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
        Assert.Contains(list, d => d.Id == 4 && d.Email == "u1@x.com");
    }

    /// <summary>
    ///   Returns BadRequest when service throws ArgumentException.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockUserService
            .Setup(s => s.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException("Invalid"));

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(result);
        var bad = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.NotNull(bad.Value);
    }

    /// <summary>
    ///   Returns Ok with default parameters.
    /// </summary>
    [Fact]
    public void GetAll_WithDefaultParameters_ReturnsOk()
    {
        var users = new List<readytohelpapi.User.Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 30, name: "Default1"),
            UserFixture.CreateOrUpdateUser(id: 31, name: "Default2"),
        };
        mockUserService.Setup(s => s.GetAllUsers(1, 10, "Name", "asc", "")).Returns(users);

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(ok.Value);
    }

    /// <summary>
    ///   Returns Ok with custom parameters.
    /// </summary>
    [Fact]
    public void GetAll_WithCustomParameters_ReturnsOk()
    {
        var users = new List<readytohelpapi.User.Models.User>
        {
            UserFixture.CreateOrUpdateUser(id: 32, name: "Custom"),
        };
        mockUserService.Setup(s => s.GetAllUsers(2, 5, "Email", "desc", "filter")).Returns(users);

        var result = controller.GetAll(2, 5, "Email", "desc", "filter");

        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(result);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(ok.Value);
    }

    /// <summary>
    ///   Returns 500 on unexpected exception on GetAll.
    /// </summary>
    [Fact]
    public void GetAll_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockUserService
            .Setup(s => s.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Database error"));

        var result = controller.GetAll();

        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(result);
        var status = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, status.StatusCode);
    }

    /// <summary>
    ///   Returns NotFound when email is not found.
    /// </summary>
    [Fact]
    public void GetUserByEmail_NotFound_ReturnsNotFound()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Returns((readytohelpapi.User.Models.User?)null);

        var result = controller.GetUserByEmail("noone@example.com");

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Returns Ok with DTO when email exists.
    /// </summary>
    [Fact]
    public void GetUserByEmail_Found_ReturnsOk_WithDto()
    {
        var user = UserFixture.CreateOrUpdateUser(id: 6, name: "Found", email: "found@example.com");
        mockUserService.Setup(s => s.GetUserByEmail("found@example.com")).Returns(user);

        var result = controller.GetUserByEmail("found@example.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Email, dto.Email);
    }

    /// <summary>
    ///   Returns BadRequest on invalid email.
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
    ///   Returns 500 on unexpected exception.
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
    ///   Returns NotFound when email is whitespace.
    /// </summary>
    [Fact]
    public void GetUserByEmail_WithWhitespaceEmail_ReturnsNotFoundOrThrows()
    {
        mockUserService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Returns((readytohelpapi.User.Models.User?)null);

        var result = controller.GetUserByEmail("   ");

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    ///   Verifies routing and authorization attributes.
    /// </summary>
    [Fact]
    public void GetUserById_HasAuthorizeAndRoutes()
    {
        var controllerType = typeof(UserApiController);
        var mi = controllerType.GetMethod("GetUserById");
        Assert.NotNull(mi);

        var routeAttrs = controllerType.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>().ToArray();
        Assert.Contains(routeAttrs, a => (a.Template ?? string.Empty).Contains("api/user", StringComparison.OrdinalIgnoreCase));

        var httpGets = mi!.GetCustomAttributes(typeof(HttpGetAttribute), true).Cast<HttpGetAttribute>().ToArray();
        Assert.True(httpGets.Length >= 1);
        var templates = httpGets.Select(a => a.Template ?? string.Empty).ToArray();
        Assert.Contains(templates, t => t.Contains("{id", StringComparison.OrdinalIgnoreCase));

        var hasAuthorize = mi.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        Assert.True(hasAuthorize);
    }
}
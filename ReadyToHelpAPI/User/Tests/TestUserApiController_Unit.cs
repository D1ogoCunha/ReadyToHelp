namespace readytohelpapi.User.Tests;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.User.Controllers;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using Xunit;

[Trait("Category", "Unit")]
public class TestUserApiController_Unit
{
    private readonly Mock<IUserService> mockService = new();
    private readonly UserApiController controller;

    public TestUserApiController_Unit()
    {
        controller = new UserApiController(mockService.Object);
    }

    private static User NewUser(
        int id = 1,
        string name = "John Doe",
        string email = "john@example.com",
        Profile profile = Profile.CITIZEN)
        => new User { Id = id, Name = name, Email = email, Profile = profile };

    [Fact]
    public void Create_NullUser_ReturnsBadRequest()
    {
        var result = controller.Create(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_ValidUser_ReturnsCreatedAtAction()
    {
        var created = NewUser(id: 42, name: "Alice", email: "alice@example.com", profile: Profile.MANAGER);
        mockService.Setup(s => s.Create(It.IsAny<User>())).Returns(created);

        var result = controller.Create(new User { Name = "Alice", Email = "alice@example.com" });

        var createdRes = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), createdRes.ActionName);
        Assert.Equal(42, createdRes.RouteValues?["id"]);
        Assert.NotNull(createdRes.Value);
    }

    [Fact]
    public void Create_EmailExists_ReturnsConflict()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Create(NewUser(id: 0));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Create_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.Create(It.IsAny<User>()))
            .Throws(new Exception("Unexpected"));

        var result = controller.Create(NewUser(id: 0));

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void Update_NullUser_ReturnsBadRequest()
    {
        var result = controller.Update(5, null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_ValidUser_ReturnsOk_WithServiceResult()
    {
        var updated = NewUser(id: 10, name: "Bob", email: "bob@example.com", profile: Profile.ADMIN);
        mockService.Setup(s => s.Update(It.Is<User>(u => u.Id == 10))).Returns(updated);

        var body = NewUser(id: 999, name: "Bob", email: "bob@example.com");
        var result = controller.Update(10, body);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(updated, ok.Value);
    }

    [Fact]
    public void Update_ServiceThrowsArgumentNullException_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.Update(It.IsAny<User>()))
            .Throws(new ArgumentNullException("user", "The user parameter cannot be null."));

        var result = controller.Update(2, NewUser(id: 2));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_EmailExists_ReturnsConflict()
    {
        mockService
            .Setup(s => s.Update(It.IsAny<User>()))
            .Throws(new ArgumentException("Email already exists"));

        var result = controller.Update(7, NewUser(id: 7));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Update_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.Update(It.IsAny<User>()))
            .Throws(new ArgumentException("invalid name"));

        var result = controller.Update(8, NewUser(id: 8));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        mockService
            .Setup(s => s.Update(It.IsAny<User>()))
            .Throws(new KeyNotFoundException("User not found"));

        var result = controller.Update(999, NewUser(id: 999));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.Update(It.IsAny<User>()))
            .Throws(new Exception("DB error"));

        var result = controller.Update(3, NewUser(id: 3));

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void Delete_Existing_ReturnsOk()
    {
        var deleted = NewUser(id: 4, name: "Carol");
        mockService.Setup(s => s.Delete(4)).Returns(deleted);

        var result = controller.Delete(4);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(deleted, ok.Value);
    }

    [Fact]
    public void Delete_NonExisting_ReturnsNotFound()
    {
        mockService.Setup(s => s.Delete(It.IsAny<int>())).Returns((User?)null!);

        var result = controller.Delete(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new ArgumentException("invalid id"));

        var result = controller.Delete(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Delete_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        mockService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new KeyNotFoundException());

        var result = controller.Delete(123);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.Delete(It.IsAny<int>()))
            .Throws(new Exception("oops"));

        var result = controller.Delete(5);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetUserById_ValidId_ReturnsOk()
    {
        var user = NewUser(id: 6, name: "Dave");
        mockService.Setup(s => s.GetUserById(6)).Returns(user);

        var result = controller.GetUserById(6);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public void GetUserById_InvalidId_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new ArgumentException("invalid"));

        var result = controller.GetUserById(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetUserById_NotFound_ReturnsNotFound()
    {
        mockService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new KeyNotFoundException());

        var result = controller.GetUserById(777);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetUserById_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.GetUserById(It.IsAny<int>()))
            .Throws(new Exception("boom"));

        var result = controller.GetUserById(9);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void Register_NullRequest_ReturnsBadRequest()
    {
        var result = controller.Register(null);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("", "e@e.com", "p")]
    [InlineData("  ", "e@e.com", "p")]
    [InlineData("John", "", "p")]
    [InlineData("John", "   ", "p")]
    [InlineData("John", "e@e.com", "")]
    [InlineData("John", "e@e.com", "   ")]
    public void Register_InvalidFields_ReturnsBadRequest(string name, string email, string password)
    {
        var req = new UserApiController.RegisterRequest(name, email, password);
        var result = controller.Register(req);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Register_ValidRequest_ReturnsCreatedAtAction()
    {
        var created = NewUser(id: 12, name: "Eve", email: "eve@example.com", profile: Profile.CITIZEN);
        mockService.Setup(s => s.Register(It.IsAny<User>())).Returns(created);

        var req = new UserApiController.RegisterRequest("Eve", "eve@example.com", "secret");
        var result = controller.Register(req);

        var createdRes = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), createdRes.ActionName);
        Assert.Equal(12, createdRes.RouteValues?["id"]);
    }

    [Fact]
    public void Register_EmailExists_ReturnsConflict()
    {
        mockService
            .Setup(s => s.Register(It.IsAny<User>()))
            .Throws(new ArgumentException("Email already exists"));

        var req = new UserApiController.RegisterRequest("Carl", "carl@example.com", "pwd");
        var result = controller.Register(req);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Register_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.Register(It.IsAny<User>()))
            .Throws(new Exception("db"));

        var req = new UserApiController.RegisterRequest("Zoe", "zoe@example.com", "pwd");
        var result = controller.Register(req);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetAll_ReturnsOkWithList()
    {
        var list = new List<User> { NewUser(id: 1), NewUser(id: 2) };
        mockService
            .Setup(s => s.GetAllUsers(1, 10, "Name", "asc", ""))
            .Returns(list);

        var result = controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<List<User>>(ok.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public void GetAll_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException("bad"));

        var result = controller.GetAll();

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetAll_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("oops"));

        var result = controller.GetAll();

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public void GetUserByEmail_NotFound_ReturnsNotFound()
    {
        mockService.Setup(s => s.GetUserByEmail("none@example.com")).Returns((User?)null);

        var result = controller.GetUserByEmail("none@example.com");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetUserByEmail_Found_ReturnsOk()
    {
        var user = NewUser(id: 21, email: "find@example.com");
        mockService.Setup(s => s.GetUserByEmail("find@example.com")).Returns(user);

        var result = controller.GetUserByEmail("find@example.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public void GetUserByEmail_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        mockService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Throws(new ArgumentException("invalid email"));

        var result = controller.GetUserByEmail("bad");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetUserByEmail_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        mockService
            .Setup(s => s.GetUserByEmail(It.IsAny<string>()))
            .Throws(new Exception("boom"));

        var result = controller.GetUserByEmail("err@example.com");

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}

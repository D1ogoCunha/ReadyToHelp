using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Common.Data;
using readytohelpapi.User.Controllers;
using readytohelpapi.User.DTOs;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///   Integration tests for UserApiController.
/// </summary>
[Trait("Category", "Integration")]
public class TestUserApiController_Integration : IClassFixture<DbFixture>
{
    private readonly AppDbContext ctx;
    private readonly IUserRepository repo;
    private readonly IUserService svc;
    private readonly UserApiController controller;

    public TestUserApiController_Integration(DbFixture fixture)
    {
        fixture.ResetDatabase();
        ctx = fixture.Context;
        repo = new UserRepository(ctx);
        svc = new UserServiceImpl(repo);
        controller = new UserApiController(svc);
    }

    [Fact]
    public void CreateUser_StoresInDatabase()
    {
        var email = $"int_create_{Guid.NewGuid():N}@example.com";
        var input = new Models.User
        {
            Name = "Int Create",
            Email = email,
            Password = "password",
            Profile = Profile.CITIZEN
        };

        var result = controller.Create(input);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetUserById), created.ActionName);
        var dto = Assert.IsType<UserResponseDto>(created.Value);
        Assert.Equal(input.Name, dto.Name);
        Assert.Equal(input.Email, dto.Email);

        var inDb = ctx.Users.AsQueryable().FirstOrDefault(u => u.Email == email);
        Assert.NotNull(inDb);
        Assert.True(inDb!.Id > 0);
    }

    [Fact]
    public void GetUserById_ReturnsStoredUser_Dto()
    {
        var u = new Models.User
        {
            Name = "John Doe",
            Email = $"john_{Guid.NewGuid():N}@example.com",
            Password = "pwd",
            Profile = Profile.CITIZEN
        };
        ctx.Users.Add(u);
        ctx.SaveChanges();

        var result = controller.GetUserById(u.Id);
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(u.Id, returned.Id);
        Assert.Equal(u.Email, returned.Email);
        Assert.Equal(u.Name, returned.Name);
    }

    [Fact]
    public void GetAll_ReturnsOkWithList_OfDto()
    {
        var u1 = new Models.User { Name = "U1", Email = $"u1_{Guid.NewGuid():N}@ex.com", Password = "p", Profile = Profile.CITIZEN };
        var u2 = new Models.User { Name = "U2", Email = $"u2_{Guid.NewGuid():N}@ex.com", Password = "p", Profile = Profile.CITIZEN };
        ctx.Users.AddRange(u1, u2);
        ctx.SaveChanges();

        var action = controller.GetAll();
        var actionResult = Assert.IsType<ActionResult<List<UserResponseDto>>>(action);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var list = Assert.IsType<List<UserResponseDto>>(ok.Value);
        Assert.True(list.Count >= 2);
        Assert.Contains(list, d => d.Email == u1.Email);
        Assert.Contains(list, d => d.Email == u2.Email);
    }
}
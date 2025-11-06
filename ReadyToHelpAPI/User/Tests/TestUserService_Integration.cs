namespace readytohelpapi.User.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using readytohelpapi.User.Tests.Fixtures;
using Xunit;

/// <summary>
///   Integration tests for UserServiceImpl.
/// </summary>
[Trait("Category", "Integration")]
public class TestUserService_Integration : IClassFixture<DbFixture>
{
    private readonly DbFixture fixture;
    private readonly IUserRepository repo;
    private readonly IUserService svc;

    /// <summary>
    ///   Initializes repository and service with in-memory DB.
    /// </summary>
    public TestUserService_Integration(DbFixture fixture)
    {
        this.fixture = fixture;
        fixture.ResetDatabase();
        repo = new UserRepository(fixture.Context);
        svc = new UserServiceImpl(repo);
    }

    private static string UniqueEmail(string prefix = "u") =>
        $"{prefix}_{Guid.NewGuid():N}@example.com";

    /// <summary>
    ///   Creates a user and verifies it is persisted.
    /// </summary>
    [Fact]
    public void Create_Persists_User_In_Database()
    {
        var input = new User
        {
            Name = "Create User",
            Email = UniqueEmail("create"),
            Password = "pass123",
            Profile = Profile.CITIZEN,
        };

        var created = svc.Create(input);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var inDb = fixture.Context.Users.FirstOrDefault(u => u.Id == created.Id);
        Assert.NotNull(inDb);
        Assert.Equal(input.Email, inDb!.Email);
    }

    /// <summary>
    ///   Ensures Register sets Profile to CITIZEN.
    /// </summary>
    [Fact]
    public void Register_Sets_Profile_To_Citizen()
    {
        var created = svc.Register(
            new User
            {
                Name = "Reg User",
                Email = UniqueEmail("reg"),
                Password = "pwd",
            }
        );

        var inDb = fixture.Context.Users.First(u => u.Id == created.Id);
        Assert.Equal(Profile.CITIZEN, inDb.Profile);
    }

    /// <summary>
    ///   Ensures Update keeps existing password when null.
    /// </summary>
    [Fact]
    public void Update_With_Null_Password_Keeps_Existing_Password()
    {
        var email = UniqueEmail("upd");
        var u = new User
        {
            Name = "Before",
            Email = email,
            Password = "oldpass",
            Profile = Profile.CITIZEN,
        };
        u = svc.Create(u);

        var before = fixture.Context.Users.AsNoTracking().First(x => x.Id == u.Id);
        var beforeHash = before.Password;

        fixture.Context.ChangeTracker.Clear();

        var updated = svc.Update(
            new User
            {
                Id = u.Id,
                Name = "After",
                Email = email,
                Password = null!,
                Profile = Profile.MANAGER,
            }
        );

        var after = fixture.Context.Users.AsNoTracking().First(x => x.Id == u.Id);

        Assert.Equal("After", updated.Name);
        Assert.Equal(beforeHash, after.Password);
        Assert.Equal(Profile.MANAGER, after.Profile);
    }

    /// <summary>
    ///   Ensures Update throws when user does not exist.
    /// </summary>
    [Fact]
    public void Update_NotFound_Throws_KeyNotFoundException()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() =>
            svc.Update(
                new User
                {
                    Id = 999999,
                    Name = "Nope",
                    Email = UniqueEmail("nope"),
                    Password = "x",
                    Profile = Profile.CITIZEN,
                }
            )
        );
        Assert.Contains("not", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Ensures Delete removes the user from the database.
    /// </summary>
    [Fact]
    public void Delete_Removes_User_From_Database()
    {
        var u = svc.Create(
            new User
            {
                Name = "To Delete",
                Email = UniqueEmail("del"),
                Password = "pwd",
                Profile = Profile.CITIZEN,
            }
        );

        var deleted = svc.Delete(u.Id);
        Assert.NotNull(deleted);

        var inDb = fixture.Context.Users.FirstOrDefault(x => x.Id == u.Id);
        Assert.Null(inDb);
    }

    /// <summary>
    ///   Ensures GetAll applies paging, sorting and filtering.
    /// </summary>
    [Fact]
    public void GetAll_Pagination_Sorting_Filtering_Works()
    {
        var u1 = svc.Create(
            new User
            {
                Name = "Alice",
                Email = UniqueEmail("ga"),
                Password = "p",
                Profile = Profile.CITIZEN,
            }
        );
        var u2 = svc.Create(
            new User
            {
                Name = "Bob",
                Email = UniqueEmail("ga"),
                Password = "p",
                Profile = Profile.CITIZEN,
            }
        );
        var u3 = svc.Create(
            new User
            {
                Name = "Carol",
                Email = UniqueEmail("ga"),
                Password = "p",
                Profile = Profile.CITIZEN,
            }
        );

        var page1 = svc.GetAllUsers(
            pageNumber: 1,
            pageSize: 2,
            sortBy: "Name",
            sortOrder: "asc",
            filter: ""
        );
        Assert.Equal(2, page1.Count);
        Assert.True(string.Compare(page1[0].Name, page1[1].Name, StringComparison.Ordinal) <= 0);
        Assert.Contains(page1, x => x.Id == u1.Id);
        Assert.Contains(page1, x => x.Id == u2.Id);
        Assert.DoesNotContain(page1, x => x.Id == u3.Id);

        var filtered = svc.GetAllUsers(1, 10, "Name", "asc", "bo");
        Assert.Contains(filtered, x => x.Id == u2.Id);
    }
}

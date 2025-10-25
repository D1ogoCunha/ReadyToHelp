// <copyright file="TestUserModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using readytohelpapi.User.Models;
using Xunit;

namespace readytohelpapi.User.Tests;

/// <summary>
///      This class contains all unit tests related to the user model.
/// </summary>
public class TestUserModel
{
    /// <summary>
    ///     Test to verify that the default constructor initializes properties correctly.
    /// </summary>
    [Fact]
    public void User_DefaultConstructor_ShouldInitializePropertiesCorrectly()
    {
        var user = new Models.User();

        Assert.Equal(0, user.Id);
        Assert.Null(user.Name);
        Assert.Null(user.Email);
        Assert.Null(user.Password);
        Assert.Equal(default, user.Profile);
    }

    /// <summary>
    ///     Test to verify that the parameterized constructor sets properties correctly.
    /// </summary>
    [Fact]
    public void User_ConstructorWithParameters_SetsPropertiesCorrectly()
    {
        var expectedId = 2;
        var expectedName = "Diogo";
        var expectedEmail = "diogo@example.com";
        var expectedPassword = "123456";
        var expectedProfile = Profile.CITIZEN;

        var user = new Models.User(
            expectedId,
            expectedName,
            expectedEmail,
            expectedPassword,
            expectedProfile
        );

        Assert.Equal(expectedId, user.Id);
        Assert.Equal(expectedName, user.Name);
        Assert.Equal(expectedEmail, user.Email);
        Assert.Equal(expectedPassword, user.Password);
        Assert.Equal(expectedProfile, user.Profile);
    }
}

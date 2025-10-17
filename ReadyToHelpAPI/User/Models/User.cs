// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace readytohelpapi.User.Models;

/// <summary>
///     Represents a user.
/// </summary>
public class User
{

    /// <summary>
    ///     Gets or sets the ID of the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the email of the user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    ///     Gets or sets the password of the user.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     Gets or sets the profile of the user.
    /// </summary>
    public Profile Profile { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="User" /> class.
    /// </summary>
    public User() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="User" /> class.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="name">The full name of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <param name="profile">The type of a profile of the user.</param>
    public User(int id, string name, string email, string password, Profile profile)
    {
        Id = id;
        Email = email;
        Name = name;
        Password = password;
        Profile = profile;
    }

}
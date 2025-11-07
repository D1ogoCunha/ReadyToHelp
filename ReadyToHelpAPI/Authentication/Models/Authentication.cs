namespace readytohelpapi.Authentication.Models;

/// <summary>
/// Represents the authentication details for a user.
/// </summary>
public class Authentication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Authentication"/> class with the specified email and password.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    public Authentication(string email, string password)
    {
        this.Email = email;
        this.Password = password;
    }

    /// <summary>
    /// Gets or sets the email of the entity.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the password of the entity.
    /// </summary>
    public string Password { get; set; }
}

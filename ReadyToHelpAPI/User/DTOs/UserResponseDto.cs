namespace readytohelpapi.User.DTOs;

using readytohelpapi.User.Models;

/// <summary>
/// Data Transfer Object for User Response.
/// </summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Profile Profile { get; set; }
}

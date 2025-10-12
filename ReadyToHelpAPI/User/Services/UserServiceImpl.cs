namespace readytohelpapi.User.Services;

using Models;

/// <summary>
///     Implements the user service operations.
/// </summary>
public class UserServiceImpl : IUserService
{
    private readonly IUserRepository userRepository;

    /// <summary>
    ///    Initializes a new instance of the <see cref="UserServiceImpl"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository instance.</param>
    public UserServiceImpl(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <summary>
    ///   Creates a user.
    /// </summary>
    public Models.User Create(Models.User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User object is null");

        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrEmpty(user.Email))
            throw new ArgumentException("Email cannot be null or empty", nameof(user.Email));

        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrEmpty(user.Name))
            throw new ArgumentException("User name cannot be null or empty", nameof(user.Name));

        if (string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrEmpty(user.Password))
            throw new ArgumentException("User password cannot be null or empty", nameof(user.Password));

        var existingUser = this.userRepository.GetUserByEmail(user.Email);

        if (existingUser != null)
            throw new ArgumentException($"Email {user.Email} already exists.");

        try
        {
            return this.userRepository.Create(user);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while trying to create a user.", e);
        }
    }

    public Models.User? GetProfile(int id)
    {
        return userRepository.GetProfile(id);
    }
}
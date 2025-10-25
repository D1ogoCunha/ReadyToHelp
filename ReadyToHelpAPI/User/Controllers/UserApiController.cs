namespace readytohelpapi.User.Controllers;

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;

/// <summary>
///   Provides API endpoints for managing users.
/// </summary>
[ApiController]
[Route("api/user")]
public class UserApiController : ControllerBase
{
    private readonly IUserService userService;

    /// <summary>
    ///    Initializes a new instance of the <see cref="UserApiController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    public UserApiController(IUserService userService)
    {
        this.userService = userService;
    }

    /// <summary>
    /// Creates a new user. Only ADMIN can call.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public ActionResult Create([FromBody] User user)
    {
        if (user is null)
            return BadRequest(new { error = "User payload is required." });

        try
        {
            var created = userService.Create(user);
            return CreatedAtAction(
                nameof(GetUserById),
                new { id = created.Id },
                new
                {
                    created.Id,
                    created.Name,
                    created.Email,
                    created.Profile,
                }
            );
        }
        catch (ArgumentException ex)
            when (ex.Message?.Contains("exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    ///   Updates an existing user. Only accessible by ADMIN users.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public ActionResult Update([FromRoute] int id, [FromBody] User user)
    {
        if (user == null)
            return BadRequest(new { error = "user_required" });

        user.Id = id;

        try
        {
            var updated = userService.Update(user);
            return Ok(updated);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
            when (ex.Message?.Contains("exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    ///   Deletes an existing user. Only accessible by ADMIN users.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public ActionResult Delete([FromRoute] int id)
    {
        try
        {
            var deleted = userService.Delete(id);
            if (deleted == null)
                return NotFound();
            return Ok(deleted);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    // GET api/user/{id}
    [HttpGet("{id:int}")]
    public ActionResult GetUserById(int id)
    {
        try
        {
            var user = userService.GetUserById(id);
            return Ok(user);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { error = "Invalid user id." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    public record RegisterRequest(string Name, string Email, string Password);

    /// <summary>
    /// Registers a new user with the CITIZEN profile. Publicly accessible.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public ActionResult Register([FromBody] RegisterRequest? req)
    {
        if (
            req is null
            || string.IsNullOrWhiteSpace(req.Name)
            || string.IsNullOrWhiteSpace(req.Email)
            || string.IsNullOrWhiteSpace(req.Password)
        )
            return BadRequest(new { error = "Name, Email and Password are required." });

        try
        {
            var created = userService.Register(
                new User
                {
                    Name = req.Name.Trim(),
                    Email = req.Email.Trim(),
                    Password = req.Password,
                    Profile = Profile.CITIZEN,
                }
            );

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = created.Id },
                new
                {
                    created.Id,
                    created.Name,
                    created.Email,
                    created.Profile,
                }
            );
        }
        catch (ArgumentException ex)
            when (ex.Message?.Contains("exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    ///   Lists users with pagination, sorting and filtering.
    /// </summary>
    [HttpGet]
    public ActionResult<List<User>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string filter = ""
    )
    {
        try
        {
            var users = userService.GetAllUsers(pageNumber, pageSize, sortBy, sortOrder, filter);
            return Ok(users);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    [HttpGet("email/{email}")]
    public ActionResult GetUserByEmail(string email)
    {
        try
        {
            var user = userService.GetUserByEmail(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }
}

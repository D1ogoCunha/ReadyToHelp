namespace readytohelpapi.User.Controllers;

using Microsoft.AspNetCore.Mvc;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;
using System;

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
    ///   Creates a new user. Only accessible by ADMIN users.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="callerProfile">The profile of the user making the request.</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Create([FromBody] User user, [FromHeader(Name = "X-User-Profile")] string? callerProfile)
    {
        if (!string.Equals(callerProfile, "ADMIN", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        try
        {
            var created = userService.Create(user);
            return CreatedAtAction(nameof(GetProfile), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) when (ex.Message?.Contains("exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
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

    // GET api/user/{id}
    [HttpGet("{id:int}")]
    public ActionResult GetProfile(int id)
    {
        var user = userService.GetProfile(id);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
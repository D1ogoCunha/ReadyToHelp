namespace readytohelpapi.User.Controllers;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.User.DTOs;
using readytohelpapi.User.Models;
using readytohelpapi.User.Services;

/// <summary>
/// Provides API endpoints for managing users.
/// </summary>
[ApiController]
[Route("api/user")]
public class UserApiController : ControllerBase
{
    private readonly IUserService userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserApiController"/> class.
    /// </summary>
    /// <param name="userService"></param>
    public UserApiController(IUserService userService)
    {
        this.userService = userService;
    }

    /// <summary>
    /// Registers a new user (CITIZEN).
    /// </summary>
    /// <param name="req"></param>
    /// <returns>The Registered user.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest? req)
    {
        try
        {
            if (req is null)
                return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email is required.");
            if (string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Password is required.");

            var created = userService.Register(
                new User
                {
                    Name = req.Name,
                    Email = req.Email,
                    Password = req.Password,
                    Profile = Profile.CITIZEN,
                }
            );

            var dto = new UserResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                Email = created.Email,
                Profile = created.Profile,
            };

            return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Creates a new user. Only ADMIN or MANAGER can call.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The created user.</returns>
    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost]
    public IActionResult Create([FromBody] User? user)
    {
        try
        {
            if (user is null)
                return BadRequest("User body is required.");

            var created = userService.Create(user);

            var dto = new UserResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                Email = created.Email,
                Profile = created.Profile,
            };

            return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing user. Only ADMIN or MANAGER can call.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns>The updated user.</returns>
    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPut("{id:int}")]
    public ActionResult Update([FromRoute] int id, [FromBody] User? user)
    {
        try
        {
            if (user is null)
                return BadRequest("User body is required.");
            user.Id = id;

            var updated = userService.Update(user);

            var dto = new UserResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Email = updated.Email,
                Profile = updated.Profile,
            };

            return Ok(dto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Deletes a user by Id. Only ADMIN or MANAGER can call.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The deleted user.</returns>
    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        try
        {
            var deleted = userService.Delete(id);
            if (deleted is null)
                return NotFound();

            var dto = new UserResponseDto
            {
                Id = deleted.Id,
                Name = deleted.Name,
                Email = deleted.Email,
                Profile = deleted.Profile,
            };

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets an user by its Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The user with the specified Id if found; otherwise a NotFound result.</returns>
    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetUserById([FromRoute] int id)
    {
        try
        {
            var u = userService.GetUserById(id);

            var dto = new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Profile = u.Profile,
            };

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets all users with pagination, sorting, and filtering.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="sortBy"></param>
    /// <param name="sortOrder"></param>
    /// <param name="filter"></param>
    /// <returns>A list of users.</returns>
    [Authorize]
    [HttpGet]
    public ActionResult<List<UserResponseDto>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string filter = ""
    )
    {
        try
        {
            var list = userService.GetAllUsers(pageNumber, pageSize, sortBy, sortOrder, filter);

            var dtoList = list.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Profile = u.Profile,
                })
                .ToList();

            return Ok(dtoList);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>The user with the specified email if found; otherwise a NotFound result.</returns>
    [Authorize]
    [HttpGet("email/{email}")]
    public IActionResult GetUserByEmail([FromRoute] string email)
    {
        try
        {
            var u = userService.GetUserByEmail(email);
            if (u is null)
                return NotFound();

            var dto = new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Profile = u.Profile,
            };

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    public record RegisterRequest(string Name, string Email, string Password);
}

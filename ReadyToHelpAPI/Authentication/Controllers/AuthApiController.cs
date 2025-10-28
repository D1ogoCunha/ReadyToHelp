using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Authentication.Service;

namespace readytohelpapi.Authentication.Controllers;


/// <summary>
/// Controller for handling authentication-related API endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthApiController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthApiController(IAuthService authService)
    {
        this.authService = authService;
    }

    /// <summary>
    /// Logs in a user for mobile access and returns a JWT token.
    /// </summary>
    /// <param name="authentication">The authentication details.</param>
    /// <returns>A JWT token if successful or an error message if not.</returns>
    [AllowAnonymous]
    [HttpPost("login/mobile")]
    public ActionResult<string> LoginMobile([FromBody] Models.Authentication? authentication)
    {
        if (authentication is null) return BadRequest("Invalid authentication details.");

        try
        {
            var token = this.authService.UserLoginMobile(authentication);
            return Ok(token);
        }
        catch (AuthenticationException ex)
        {
            return this.Unauthorized(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return this.StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Logs in a user for web access and returns a JWT token.
    /// </summary>
    /// <param name="authentication">The authentication details.</param>
    /// <returns>A JWT token if successful or an error message if not.</returns>
    [AllowAnonymous]
    [HttpPost("login/web")]
    public ActionResult<string> LoginWeb([FromBody] Models.Authentication? authentication)
    {
        if (authentication is null) return BadRequest("Invalid authentication details.");

        try
        {
            var token = this.authService.UserLoginWeb(authentication);
            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (AuthenticationException ex)
        {
            return this.Unauthorized(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return this.StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Refreshes a JWT token. The existing token must be provided in the Authorization header as a Bearer token.
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("refresh-token")]
    public ActionResult<string> RefreshToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var token = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token is required.");

        var newToken = authService.RefreshToken(token);
        if (string.IsNullOrEmpty(newToken)) return Unauthorized("Invalid or expired token.");

        return Ok(newToken);
    }

    /// <summary>
    /// Logs out the user by revoking the provided JWT token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Authorization Bearer token is required in the Authorization header.");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Invalid Bearer token.");

        try
        {
            authService.RevokeToken(token);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while revoking token.");
        }
    }
    
}
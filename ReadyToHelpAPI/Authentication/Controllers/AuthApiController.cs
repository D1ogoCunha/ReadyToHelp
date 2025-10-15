using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Authentication.Service;




namespace readytohelpapi.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthApiController(IAuthService authService)
    {
        this.authService = authService;
    }

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
    
}
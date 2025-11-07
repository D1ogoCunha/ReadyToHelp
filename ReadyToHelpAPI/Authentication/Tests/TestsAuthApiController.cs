namespace readytohelpapi.Authentication.Tests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using readytohelpapi.Authentication.Controllers;
using readytohelpapi.Authentication.Service;
using AuthDto = readytohelpapi.Authentication.Models.Authentication;
using AuthException = System.Security.Authentication.AuthenticationException;
using Xunit;

/// <summary>
/// Unit tests for the AuthApiController.
/// </summary>
[Trait("Category", "Unit")]
public class TestsAuthApiController
{
    /// <summary>
    /// Helper method to create an instance of AuthApiController with a mocked IAuthService.
    /// </summary>
    /// <param name="svc">The mocked IAuthService.</param>
    /// <returns>An instance of AuthApiController.</returns>
    private static AuthApiController MakeController(IAuthService svc)
    {
        return new AuthApiController(svc)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    /// <summary>
    ///  Tests that LoginMobile returns Ok when provided valid authentication details.
    /// </summary>
    [Fact]
    public void LoginMobile_ReturnsOk_OnValid()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginMobile(It.IsAny<AuthDto>())).Returns("tok");
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginMobile(new AuthDto("citizen@mail.com", "1234"));

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        Assert.Equal("tok", Assert.IsType<string>(ok.Value));
    }

    /// <summary>
    /// Tests that LoginMobile returns Unauthorized when the authentication service throws an AuthException.
    /// </summary>
    [Fact]
    public void LoginMobile_ReturnsUnauthorized_OnAuthException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginMobile(It.IsAny<AuthDto>())).Throws(new AuthException("bad"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginMobile(new AuthDto("e@mail.com", "p"));

        Assert.IsType<UnauthorizedObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that LoginMobile returns BadRequest when the authentication service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void LoginMobile_ReturnsBadRequest_OnNullPayload()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);

        var res = ctrl.LoginMobile(null);

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that LoginWeb returns Ok when provided valid authentication details.
    /// </summary>
    [Fact]
    public void LoginWeb_ReturnsOk_OnValid()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginWeb(It.IsAny<AuthDto>())).Returns("tok");
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginWeb(new AuthDto("admin@mail.com", "1234"));

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        Assert.Equal("tok", Assert.IsType<string>(ok.Value));
    }

    /// <summary>
    /// Tests that LoginWeb returns Forbid when the authentication service throws an UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public void LoginWeb_ReturnsForbid_OnUnauthorizedAccess()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginWeb(It.IsAny<AuthDto>())).Throws(new UnauthorizedAccessException());
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginWeb(new AuthDto("citizen@mail.com", "1234"));

        Assert.IsType<ForbidResult>(res.Result);
    }

    [Fact]
    public void LoginWeb_ReturnsUnauthorized_OnAuthException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginWeb(It.IsAny<AuthDto>())).Throws(new AuthException("bad"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginWeb(new AuthDto("e@mail.com", "p"));

        Assert.IsType<UnauthorizedObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that LoginWeb returns BadRequest when the authentication service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void LoginWeb_ReturnsBadRequest_OnArgumentException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginWeb(It.IsAny<AuthDto>())).Throws(new ArgumentException("invalid"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginWeb(new AuthDto("e@mail.com", "p"));

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that RefreshToken returns Ok with a new token when provided a valid Authorization header.
    /// </summary>
    [Fact]
    public void RefreshToken_ReturnsOk_OnValidHeader()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RefreshToken("abc")).Returns("newtok");
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer abc";

        var res = ctrl.RefreshToken();

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        Assert.Equal("newtok", Assert.IsType<string>(ok.Value));
    }

    /// <summary>
    /// Tests that RefreshToken returns Unauthorized when the authentication service returns an empty string.
    /// </summary>
    [Fact]
    public void RefreshToken_ReturnsUnauthorized_OnServiceEmpty()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RefreshToken("abc")).Returns(string.Empty);
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer abc";

        var res = ctrl.RefreshToken();

        Assert.IsType<UnauthorizedObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that RefreshToken returns BadRequest when the Authorization header is missing.
    /// </summary>
    [Fact]
    public void RefreshToken_ReturnsBadRequest_WhenMissingHeader()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);

        var res = ctrl.RefreshToken();

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that RefreshToken returns BadRequest when the Authorization header is missing.
    /// </summary>
    [Fact]
    public void LoginMobile_ReturnsBadRequest_OnArgumentException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginMobile(It.IsAny<AuthDto>()))
           .Throws(new ArgumentException("invalid"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginMobile(new AuthDto("e@mail.com", "p"));

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that LoginMobile returns 500 when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public void LoginMobile_Returns500_OnUnexpectedException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginMobile(It.IsAny<AuthDto>()))
           .Throws(new Exception("boom"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginMobile(new AuthDto("e@mail.com", "p"));

        var obj = Assert.IsType<ObjectResult>(res.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    /// <summary>
    /// Tests that LoginWeb returns BadRequest when provided a null payload.
    /// </summary>
    [Fact]
    public void LoginWeb_ReturnsBadRequest_OnNullPayload()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);

        var res = ctrl.LoginWeb(null);

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that LoginWeb returns 500 when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public void LoginWeb_Returns500_OnUnexpectedException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UserLoginWeb(It.IsAny<AuthDto>()))
           .Throws(new Exception("boom"));
        var ctrl = MakeController(svc.Object);

        var res = ctrl.LoginWeb(new AuthDto("e@mail.com", "p"));

        var obj = Assert.IsType<ObjectResult>(res.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    /// <summary>
    /// Tests that RefreshToken returns BadRequest when the Authorization header does not start with "Bearer".
    /// </summary>
    [Fact]
    public void RefreshToken_ReturnsBadRequest_OnHeaderWithoutBearerPrefix()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);
        ctrl.HttpContext.Request.Headers.Authorization = "abc"; // sem 'Bearer '

        var res = ctrl.RefreshToken();

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that RefreshToken returns BadRequest when the Bearer token is empty or whitespace.
    /// </summary>
    [Fact]
    public void RefreshToken_ReturnsBadRequest_OnEmptyBearerToken()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer    "; // só espaços

        var res = ctrl.RefreshToken();

        Assert.IsType<BadRequestObjectResult>(res.Result);
    }

    /// <summary>
    /// Tests that RefreshToken returns 500 when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public void RefreshToken_AcceptsCaseInsensitiveBearer()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RefreshToken("AbC")).Returns("newtok");
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "bearer AbC"; // case-insensitive

        var res = ctrl.RefreshToken();

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        Assert.Equal("newtok", Assert.IsType<string>(ok.Value));
    }

    /// <summary>
    /// Tests that RefreshToken trims whitespace around the Bearer token.
    /// </summary>
    [Fact]
    public void RefreshToken_TrimsTokenWhitespace()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RefreshToken("abc")).Returns("newtok");
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer    abc    ";

        var res = ctrl.RefreshToken();

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        Assert.Equal("newtok", Assert.IsType<string>(ok.Value));
    }

    /// <summary>
    /// Tests that Logout returns NoContent when provided a valid Authorization header.
    /// </summary>
    [Fact]
    public void Logout_ReturnsNoContent_OnValidHeader()
    {
        var svc = new Mock<IAuthService>();
        // RevokeToken is void; setup as verifiable
        svc.Setup(s => s.RevokeToken("abc")).Verifiable();
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer abc";

        var res = ctrl.Logout();

        Assert.IsType<NoContentResult>(res);
        svc.Verify(s => s.RevokeToken("abc"), Times.Once);
    }

    /// <summary>
    /// Tests that Logout returns BadRequest when the Authorization header is missing.
    /// </summary>
    [Fact]
    public void Logout_ReturnsBadRequest_WhenMissingHeader()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);

        var res = ctrl.Logout();

        Assert.IsType<BadRequestObjectResult>(res);
    }

    /// <summary>
    /// Tests that Logout returns BadRequest when the Authorization header does not start with "Bearer".
    /// </summary>
    [Fact]
    public void Logout_ReturnsBadRequest_OnHeaderWithoutBearerPrefix()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);
        ctrl.HttpContext.Request.Headers.Authorization = "abc"; // sem 'Bearer '

        var res = ctrl.Logout();

        Assert.IsType<BadRequestObjectResult>(res);
    }

    /// <summary>
    /// Tests that Logout returns BadRequest when the Bearer token is empty or whitespace.
    /// </summary>
    [Fact]
    public void Logout_ReturnsBadRequest_OnEmptyBearerToken()
    {
        var ctrl = MakeController(new Mock<IAuthService>().Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer    "; // só espaços

        var res = ctrl.Logout();

        Assert.IsType<BadRequestObjectResult>(res);
    }

    /// <summary>
    /// Tests that Logout returns BadRequest when the authentication service throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Logout_ReturnsBadRequest_OnServiceThrowsArgumentException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RevokeToken("abc")).Throws(new ArgumentException("invalid"));
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer abc";

        var res = ctrl.Logout();

        Assert.IsType<BadRequestObjectResult>(res);
    }

    /// <summary>
    /// Tests that Logout returns 500 when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public void Logout_Returns500_OnUnexpectedException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RevokeToken("abc")).Throws(new Exception("boom"));
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "Bearer abc";

        var res = ctrl.Logout();

        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }

    /// <summary>
    /// Tests that Logout accepts case-insensitive 'Bearer' and trims whitespace around the token.
    /// </summary>
    [Fact]
    public void Logout_AcceptsCaseInsensitiveBearer_AndTrimsToken()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RevokeToken("AbC")).Verifiable();
        var ctrl = MakeController(svc.Object);
        ctrl.HttpContext.Request.Headers.Authorization = "bearer    AbC   ";

        var res = ctrl.Logout();

        Assert.IsType<NoContentResult>(res);
        svc.Verify(s => s.RevokeToken("AbC"), Times.Once);
    }
}
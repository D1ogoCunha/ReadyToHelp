// <copyright file="TesteJwtUtil.cs" company="ReadyToHelp">
// Copyright (c) ReadyToHelp. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using readytohelpapi.Authentication.Service;
using Xunit;

namespace ReadyToHelpAPI.Tests.Authentication;

/// <summary>
///     Unit tests for <see cref="JwtUtility" />.
/// </summary>
[Trait("Category", "Unit")]
public class TesteJwtUtil
{
    /// <summary>
    /// Test ConvertJwtStringToJwtSecurityToken with a valid JWT string.
    /// </summary>
    [Fact]
    public void ConvertJwtStringToJwtSecurityToken_ShouldReturnToken_WhenValidJwtString()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: "ReadyToHelp",
            audience: "ReadyToHelpUsers",
            claims: [],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: null // assinatura não é validada aqui
        );
        var tokenString = handler.WriteToken(token);

        // Act
        var result = JwtUtility.ConvertJwtStringToJwtSecurityToken(tokenString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ReadyToHelp", result!.Issuer);
    }

    /// <summary>
    /// Test ConvertJwtStringToJwtSecurityToken with an invalid JWT string.
    /// </summary>
    [Fact]
    public void ConvertJwtStringToJwtSecurityToken_ShouldReturnNull_WhenInvalidJwtString()
    {
        // Arrange
        var invalidToken = "this.is.not.a.jwt";

        // Act
        var result = JwtUtility.ConvertJwtStringToJwtSecurityToken(invalidToken);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test DecodeJwt with a valid JwtSecurityToken.
    /// </summary>
    [Fact]
    public void DecodeJwt_ShouldReturnDecodedObject_WhenValidToken()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim("role", "ADMIN"),
            new System.Security.Claims.Claim("email", "admin@mail.com")
        };

        var token = new JwtSecurityToken(
            issuer: "ReadyToHelp",
            audience: "ReadyToHelpUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1)
        );

        var result = JwtUtility.DecodeJwt(token);

        Assert.NotNull(result);
        var decoded = result!.GetType().GetProperty("issuer")!.GetValue(result, null);
        Assert.Equal("ReadyToHelp", decoded);
    }

    /// <summary>
    /// Test DecodeJwt with a null JwtSecurityToken.
    /// </summary>
    [Fact]
    public void DecodeJwt_ShouldReturnNull_WhenTokenIsNull()
    {
        var result = JwtUtility.DecodeJwt(null);

        Assert.Null(result);
    }

    /// <summary>
    /// Test ReturnJwtData with a valid JWT string.
    /// </summary>
    [Fact]
    public void ReturnJwtData_ShouldReturnDecodedData_WhenValidTokenString()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: "ReadyToHelp",
            audience: "ReadyToHelpUsers",
            claims: new[]
            {
                new System.Security.Claims.Claim("role", "MANAGER")
            },
            expires: DateTime.UtcNow.AddHours(1)
        );
        var tokenString = handler.WriteToken(token);

        var result = JwtUtility.ReturnJwtData(tokenString);

        Assert.NotNull(result);
        var type = result!.GetType();
        var issuer = type.GetProperty("issuer")!.GetValue(result, null);
        Assert.Equal("ReadyToHelp", issuer);
    }

    /// <summary>
    /// Test ReturnJwtData with an invalid JWT string.
    /// </summary>
    [Fact]
    public void ReturnJwtData_ShouldReturnNull_WhenInvalidTokenString()
    {
        var invalidToken = "invalid.token.string";

        var result = JwtUtility.ReturnJwtData(invalidToken);

        Assert.Null(result);
    }
}

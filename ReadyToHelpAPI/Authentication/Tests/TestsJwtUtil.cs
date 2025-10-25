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
public class TesteJwtUtil
{
    // -----------------------------------------
    // ✅ TEST: ConvertJwtStringToJwtSecurityToken (valid token)
    // -----------------------------------------
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

    // -----------------------------------------
    // ❌ TEST: ConvertJwtStringToJwtSecurityToken (invalid token)
    // -----------------------------------------
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

    // -----------------------------------------
    // ✅ TEST: DecodeJwt (valid token)
    // -----------------------------------------
    [Fact]
    public void DecodeJwt_ShouldReturnDecodedObject_WhenValidToken()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
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

        // Act
        var result = JwtUtility.DecodeJwt(token);

        // Assert
        Assert.NotNull(result);
        var decoded = result!.GetType().GetProperty("issuer")!.GetValue(result, null);
        Assert.Equal("ReadyToHelp", decoded);
    }

    // -----------------------------------------
    // ❌ TEST: DecodeJwt (null token)
    // -----------------------------------------
    [Fact]
    public void DecodeJwt_ShouldReturnNull_WhenTokenIsNull()
    {
        // Act
        var result = JwtUtility.DecodeJwt(null);

        // Assert
        Assert.Null(result);
    }

    // -----------------------------------------
    // ✅ TEST: ReturnJwtData (valid token string)
    // -----------------------------------------
    [Fact]
    public void ReturnJwtData_ShouldReturnDecodedData_WhenValidTokenString()
    {
        // Arrange
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

        // Act
        var result = JwtUtility.ReturnJwtData(tokenString);

        // Assert
        Assert.NotNull(result);
        var type = result!.GetType();
        var issuer = type.GetProperty("issuer")!.GetValue(result, null);
        Assert.Equal("ReadyToHelp", issuer);
    }

    // -----------------------------------------
    // ❌ TEST: ReturnJwtData (invalid token)
    // -----------------------------------------
    [Fact]
    public void ReturnJwtData_ShouldReturnNull_WhenInvalidTokenString()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var result = JwtUtility.ReturnJwtData(invalidToken);

        // Assert
        Assert.Null(result);
    }
}

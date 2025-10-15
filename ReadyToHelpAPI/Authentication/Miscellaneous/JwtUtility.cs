using System.IdentityModel.Tokens.Jwt;

namespace readytohelpapi.Authentication.Service;

public static class JwtUtility
{
    /// <summary>
    ///     Converts a JWT string to a JwtSecurityToken object.
    /// </summary>
    /// <param name="jwt">The JWT string to be converted.</param>
    /// <returns>The JwtSecurityToken object.</returns>
    public static JwtSecurityToken? ConvertJwtStringToJwtSecurityToken(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt)) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(jwt);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Decodes the JwtSecurityToken and returns its claims, issuer, audience, etc.
    /// </summary>
    /// <param name="jwt">The JwtSecurityToken to be decoded.</param>
    /// <returns>Decoded JWT information.</returns>
    public static object? DecodeJwt(JwtSecurityToken? jwt)
    {
        if (jwt == null) return null;

        var keyId = jwt.Header?.Kid;
        var audience = jwt.Audiences?.ToList() ?? new List<string>();
        var claims = jwt.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return new
        {
            key = keyId,
            issuer = jwt.Issuer,
            audience,
            claims,
            subject = jwt.Subject,
            validFrom = jwt.ValidFrom,
            validTo = jwt.ValidTo
        };
    }

    /// <summary>
    ///     Returns decoded JWT data.
    /// </summary>
    /// <param name="jwt">The JWT string to decode.</param>
    /// <returns>Decoded JWT data.</returns>
    public static object? ReturnJwtData(string? jwt)
    {
        var token = ConvertJwtStringToJwtSecurityToken(jwt);
        return DecodeJwt(token);
    }
}
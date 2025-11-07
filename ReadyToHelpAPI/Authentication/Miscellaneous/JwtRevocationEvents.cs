namespace readytohelpapi.Authentication.Miscellaneous;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

/// <summary>
///   Class responsible for handling JWT revocation events.
/// </summary>
public static class JwtRevocationEvents
{
    /// <summary>
    ///   Creates JwtBearerEvents with token revocation checking.
    /// </summary>
    public static JwtBearerEvents Create() =>
        new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                try
                {
                    var reqPath = context.HttpContext.Request.Path.Value ?? string.Empty;
                    if (reqPath.StartsWith("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
                        return;

                    var cache =
                        context.HttpContext.RequestServices.GetService(typeof(IDistributedCache))
                        as IDistributedCache;
                    if (cache == null)
                        return;

                    var jti =
                        context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
                        ?? (context.SecurityToken as JwtSecurityToken)?.Id;
                    if (string.IsNullOrWhiteSpace(jti))
                        return;

                    var key = $"revoked_tokens:{jti}";
                    var revoked = await cache.GetStringAsync(key);
                    if (!string.IsNullOrWhiteSpace(revoked))
                    {
                        context.Fail("Token has been revoked.");
                    }
                }
                catch (Exception ex)
                {
                    var loggerFactory =
                        context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory))
                        as ILoggerFactory;
                    loggerFactory
                        ?.CreateLogger("JwtBearer")
                        .LogWarning(ex, "Error checking token revocation; skipping check.");
                }
            },
        };
}

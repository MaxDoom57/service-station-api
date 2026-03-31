using Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.Middlewares
{
    public class JwtSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IUserRequestContext userContext,
            ITokenActivityService tokenActivity,
            ITokenBlacklistService tokenBlacklist)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                // ─── BLACKLIST GATE ──────────────────────────────────────────────────────
                // If the token has been revoked (e.g. after rotation or logout),
                // reject immediately — do NOT forward to the next middleware or controller.
                if (tokenBlacklist.IsTokenBlacklisted(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"error\":\"Token has been revoked. Please login again.\"}");
                    return;   // ← pipeline stops here
                }
                // ────────────────────────────────────────────────────────────────────────

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var userId = jwt.Claims.FirstOrDefault(c => c.Type == "UsrId")?.Value;
                    var companyKey = jwt.Claims.FirstOrDefault(c => c.Type == "CKy")?.Value;
                    var projectKey = jwt.Claims.FirstOrDefault(c => c.Type == "PrjKy")?.Value;

                    if (!string.IsNullOrEmpty(userId) &&
                        !string.IsNullOrEmpty(companyKey) &&
                        !string.IsNullOrEmpty(projectKey))
                    {
                        userContext.UserId = userId;
                        userContext.CompanyKey = int.Parse(companyKey);
                        userContext.ProjectKey = int.Parse(projectKey);

                        // Bump the sliding-window activity tracker so the
                        // refresh endpoint knows this token was recently used.
                        tokenActivity.Touch(token);
                    }
                }
                catch
                {
                    // Token unreadable — let the JWT bearer middleware handle it.
                }
            }
            else
            {
                // ─── PUBLIC WEBSITE KEY GATE ─────────────────────────────────────────────
                // For public (website) endpoints that use [AllowAnonymous], the frontend
                // sends X-Company-Key and X-Project-Key headers instead of a JWT token.
                // Populate the user context so DynamicDbContextFactory can select the
                // correct tenant database.
                var companyKeyHeader = context.Request.Headers["X-Company-Key"].FirstOrDefault();
                var projectKeyHeader = context.Request.Headers["X-Project-Key"].FirstOrDefault();

                if (int.TryParse(companyKeyHeader, out var companyKeyValue) &&
                    int.TryParse(projectKeyHeader, out var projectKeyValue) &&
                    companyKeyValue > 0 && projectKeyValue > 0)
                {
                    userContext.CompanyKey = companyKeyValue;
                    userContext.ProjectKey = projectKeyValue;
                }
                // ────────────────────────────────────────────────────────────────────────
            }

            await _next(context);
        }
    }
}


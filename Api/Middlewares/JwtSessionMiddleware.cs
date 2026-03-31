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

            // --- FALLBACK FOR ANONYMOUS ENDPOINTS ---
            // The DynamicDbContextFactory requires CompanyKey and ProjectKey.
            // If a token wasn't provided (e.g., AllowAnonymous), we can fallback to custom headers.
            if (userContext.CompanyKey <= 0 || userContext.ProjectKey <= 0)
            {
                var cKyHeader = context.Request.Headers["CKy"].FirstOrDefault();
                var prjKyHeader = context.Request.Headers["PrjKy"].FirstOrDefault();

                // If headers aren't sent, you can default them to 1 for your main company/project
                userContext.CompanyKey = int.TryParse(cKyHeader, out int hCky) ? hCky : 1;
                userContext.ProjectKey = int.TryParse(prjKyHeader, out int hPrjKy) ? hPrjKy : 1;
            }

            await _next(context);
        }
    }
}


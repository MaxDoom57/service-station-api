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

        public async Task InvokeAsync(HttpContext context, IUserRequestContext userContext)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

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
                    }
                }
                catch
                {
                    // Token invalid or unreadable; do not set session context
                }
            }

            await _next(context);
        }
    }
}

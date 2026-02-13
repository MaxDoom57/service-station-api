using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace Api.Middlewares
{
    public class MockResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MockResponseMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment env)
        {
            _next = next;
            _configuration = configuration;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if Mock Mode is enabled in appsettings.json
            var isMockEnabled = _configuration.GetValue<bool>("MockApiResponses");

            if (!isMockEnabled)
            {
                await _next(context);
                return;
            }

            // Construct the mock file name based on the request method and path.
            // Replace / with _ to make it a valid filename.
            // Example Request: GET /api/ssms/v0.1/calendar/unavailable-dates
            // Mock File: GET_api_ssms_v0.1_calendar_unavailable-dates.json
            
            var method = context.Request.Method.ToUpper();
            var path = context.Request.Path.Value?.Trim('/').Replace('/', '_');
            
            if (string.IsNullOrEmpty(path))
            {
                path = "root";
            }

            var fileName = $"{method}_{path}.json";
            var filePath = Path.Combine(_env.ContentRootPath, "MockData", fileName);

            if (File.Exists(filePath))
            {
                // If a matching mock file exists, return its content
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(await File.ReadAllTextAsync(filePath));
                return; // Stop the pipeline here, do not call _next()
            }

            // If no mock file is found, continue to the next middleware (likely the controller)
            // This allows hybrid behavior or falling back to real implementation if mock is missing.
            await _next(context);
        }
    }
}

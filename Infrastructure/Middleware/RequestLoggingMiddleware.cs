using System.Diagnostics;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context; // Assuming IDynamicDbContextFactory is here
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestTime = DateTime.Now;

            // Buffer the request to read body
            context.Request.EnableBuffering();
            var requestBody = await ReadRequestBody(context.Request);
            context.Request.Body.Position = 0;

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Read Response
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                
                await responseBodyStream.CopyToAsync(originalBodyStream);

                // Log to Database using a NEW scope because the original context might be disposed or problematic in some async flows, 
                // but usually inside middleware 'Invoke', we can use the passed 'serviceProvider' to create a scope.
                // However, we want to catch errors in logging without failing the request if possible, 
                // though user asked not to change logic, logging failure shouldn't crash app.
                try
                {
                    // To avoid resolving scoped services from root, use the passed 'serviceProvider' or CreateScope?
                    // Middleware is singleton, dependency injection in Invoke is the way to get scoped services.
                    // But 'IDynamicDbContextFactory' might differ. Let's see how 'ReservationService' gets it.
                    // ReservationService gets 'IDynamicDbContextFactory' injected.
                    // It seems IDynamicDbContextFactory is Singleton or Scoped. 
                    // Let's resolve 'IDynamicDbContextFactory' and 'IUserRequestContext'.
                    
                    /* 
                       Wait, to get the User Context which is likely filled during the request, 
                       we should use the current request services.
                    */
                    
                    var factory = context.RequestServices.GetService<IDynamicDbContextFactory>();
                    var userContext = context.RequestServices.GetService<IUserRequestContext>();

                    if (factory != null)
                    {
                        using var db = await factory.CreateDbContextAsync();
                        
                        var log = new ApiRequestLog
                        {
                            RequestPath = context.Request.Path,
                            Method = context.Request.Method,
                            QueryString = context.Request.QueryString.ToString(),
                            RequestBody = Truncate(requestBody, 5000), // Limit size?
                            ResponseStatusCode = context.Response.StatusCode,
                            ResponseBody = Truncate(responseBody, 5000),
                            RequestTime = requestTime,
                            DurationMs = duration,
                            DeviceDetails = GetDeviceDetails(context),
                            UserId = ParseIntOrNull(userContext?.UserId),
                            CompanyKey = userContext?.CompanyKey
                        };

                        db.ApiRequestLogs.Add(log);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log request to database.");
                }
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private string GetDeviceDetails(HttpContext context)
        {
            var sb = new StringBuilder();
            if (context.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                sb.AppendLine($"User-Agent: {userAgent}");
            }
            sb.AppendLine($"IP: {context.Connection.RemoteIpAddress}");
            return sb.ToString();
        }

        private string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private int? ParseIntOrNull(string? val)
        {
            if (int.TryParse(val, out int res)) return res;
            return null;
        }
    }
}

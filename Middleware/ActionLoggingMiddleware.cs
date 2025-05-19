using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VstupenkyWeb.Middleware
{
    public class ActionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActionLoggingMiddleware> _logger;

        public ActionLoggingMiddleware(RequestDelegate next, ILogger<ActionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get user information
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var userName = context.User.Identity?.Name ?? "Anonymous";

            // Get request information
            var requestPath = context.Request.Path;
            var httpMethod = context.Request.Method;
            var timestamp = DateTime.UtcNow;

            // Log the action
            _logger.LogInformation(
                "User {UserId} ({UserName}) performed action {HttpMethod} {RequestPath} at {Timestamp}",
                userId, userName, httpMethod, requestPath, timestamp);

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
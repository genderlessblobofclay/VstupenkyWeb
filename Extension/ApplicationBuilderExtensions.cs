using Microsoft.AspNetCore.Builder;
using VstupenkyWeb.Middleware;

namespace VstupenkyWeb.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseActionLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActionLoggingMiddleware>();
        }
    }
}
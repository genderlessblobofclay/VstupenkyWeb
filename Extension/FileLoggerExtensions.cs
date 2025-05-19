using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using VstupenkyWeb.Logging; // Replace with your actual namespace

namespace VstupenkyWeb.Extensions
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(
            this ILoggingBuilder builder,
            IConfigurationSection configuration)
        {
            builder.AddConfiguration(configuration);

            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

            return builder;
        }
    }
}
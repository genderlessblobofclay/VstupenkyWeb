using Microsoft.Extensions.Logging;

namespace VstupenkyWeb.Logging
{
    public class ErrorFileLoggerOptions
    {
        public string Path { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Error; // Default to Error level
        public int? MaxFileSize { get; set; }
    }
}
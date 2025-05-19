using Microsoft.Extensions.Logging;

namespace VstupenkyWeb.Logging
{
    public class FileLoggerOptions
    {
        public string Path { get; set; }
        public int? MaxFileSize { get; set; }
        public LogLevel LogLevel { get; set; } // Add this line
    }
}
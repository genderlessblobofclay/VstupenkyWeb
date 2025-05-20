using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace VstupenkyWeb.Logging
{
    [ProviderAlias("ErrorFile")]
    public class FileErrorLoggerProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<ErrorFileLoggerOptions> _options;
        private readonly string _filePath;
        private readonly int? _maxFileSize;
        private readonly string _logDirectory;

        public FileErrorLoggerProvider(IOptionsMonitor<ErrorFileLoggerOptions> options)
        {
            _options = options;
            _filePath = options.CurrentValue.Path;

            if (string.IsNullOrEmpty(_filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(_filePath));
            }

            _maxFileSize = options.CurrentValue.MaxFileSize;

            // Ensure the log directory exists
            _logDirectory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(_logDirectory) && !Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, new FileLoggerOptions {
                Path = _filePath,
                LogLevel = LogLevel.Error,
                MaxFileSize = _maxFileSize
            }, _filePath, _maxFileSize);
        }

        public void Dispose()
        {
        }
    }
}
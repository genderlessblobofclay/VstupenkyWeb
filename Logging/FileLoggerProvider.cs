using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace VstupenkyWeb.Logging
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<FileLoggerOptions> _options;
        private readonly string _filePath;
        private readonly int? _maxFileSize;
        private readonly string _logDirectory;

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
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
            return new FileLogger(categoryName, _options.CurrentValue, _filePath, _maxFileSize);
        }

        public void Dispose()
        {
        }
    }
}
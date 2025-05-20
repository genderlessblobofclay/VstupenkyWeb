using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace VstupenkyWeb.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly FileLoggerOptions _options;
        private readonly string _filePath;
        private readonly int? _maxFileSize;

        public FileLogger(string categoryName, FileLoggerOptions options, string filePath, int? maxFileSize)
        {
            _categoryName = categoryName;
            _options = options;
            _filePath = options.Path ?? throw new ArgumentNullException(nameof(options.Path)); // Ensure filePath is not null
            _maxFileSize = options.MaxFileSize;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _options.LogLevel;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // Check if the log file exceeds the maximum file size
            if (_maxFileSize.HasValue)
            {
                FileInfo fileInfo = new FileInfo(_filePath);
                if (fileInfo.Exists && fileInfo.Length > _maxFileSize.Value)
                {
                    // Optionally, you can implement log rotation here
                    // For simplicity, we'll just skip logging if the file is too large
                    return;
                }
            }

            // Write the log message to the file
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(_filePath, append: true))
                {
                    streamWriter.WriteLine($"{DateTime.UtcNow} [{logLevel}] {_categoryName} - {message}");
                    if (exception != null)
                    {
                        streamWriter.WriteLine(exception.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file writing
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
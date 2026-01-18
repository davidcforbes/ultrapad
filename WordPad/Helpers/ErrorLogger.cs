using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace WordPad.Helpers
{
    /// <summary>
    /// Simple error logging utility for UltraPad
    /// </summary>
    public static class ErrorLogger
    {
        private static readonly string LogFileName = "ultrapad_errors.log";

        /// <summary>
        /// Log an exception with context information
        /// </summary>
        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] {context}\n" +
                              $"Exception: {ex.GetType().Name}\n" +
                              $"Message: {ex.Message}\n" +
                              $"StackTrace: {ex.StackTrace}\n" +
                              $"---\n\n";

                // Write to debug output for development
                Debug.WriteLine($"ERROR: {context} - {ex.Message}");
                Debug.WriteLine(ex.StackTrace);

                // Also try to write to a log file in LocalFolder
                _ = WriteToLogFileAsync(logEntry);
            }
            catch
            {
                // Fail silently if logging fails to avoid cascading errors
                Debug.WriteLine("Failed to log exception");
            }
        }

        /// <summary>
        /// Write log entry to file asynchronously
        /// </summary>
        private static async System.Threading.Tasks.Task WriteToLogFileAsync(string logEntry)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var logFile = await localFolder.CreateFileAsync(LogFileName,
                    CreationCollisionOption.OpenIfExists);

                await FileIO.AppendTextAsync(logFile, logEntry);
            }
            catch
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string message, string context = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] WARNING: {context} - {message}\n";

                Debug.WriteLine($"WARNING: {context} - {message}");
                _ = WriteToLogFileAsync(logEntry);
            }
            catch
            {
                // Fail silently
            }
        }
    }
}

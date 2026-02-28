using System;
using System.IO;

namespace StudentManagement.Logging
{
    public sealed class FileLogger : IAppLogger
    {
        private static readonly object _lockObj = new object();
        private readonly string _filePath;

        public FileLogger() : this("log.txt") { }

        public FileLogger(string filePath)
        {
            _filePath = filePath;

            
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "");
            }
        }

        public void Info(string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            lock (_lockObj) 
            {
                File.AppendAllText(_filePath, line + Environment.NewLine);
            }
        }
    }
}
using System;
using System.IO;

namespace StudentManagement.Logging
{
    public sealed class FileLogger : IAppLogger
    {
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
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string line = $"{timestamp} - {message}";
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}
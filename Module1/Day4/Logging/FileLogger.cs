using System;
using System.IO;
using System.Text;
using System.Threading;

namespace StudentManagement.Logging
{
    public sealed class FileLogger : IAppLogger
    {
     
        private static readonly object _globalLock = new();

        private readonly string _filePath;
        private readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public FileLogger() : this("log.txt") { }

        public FileLogger(string filePath)
        {
            _filePath = filePath;
         
            var dir = Path.GetDirectoryName(Path.GetFullPath(_filePath));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
            {
                using var _ = File.Create(_filePath);
            }
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        private void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";

            const int maxRetries = 5;
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    lock (_globalLock)
                    {
                        using var fs = new FileStream(
                            _filePath,
                            FileMode.Append,
                            FileAccess.Write,
                            FileShare.Read
                        );
                        using var sw = new StreamWriter(fs, _encoding);
                        sw.Write(line);
                        sw.Flush();
                    }
                    break;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    Thread.Sleep(10 * (attempt + 1));
                }
            }
        }
    }
}
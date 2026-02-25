using System.Text;

namespace StudentManagement.Logging;

public sealed class FileLogger : IAppLogger
{
    private readonly string _logDirectory;
    private readonly object _lock = new();
    private StreamWriter? _writer;
    private DateTime _currentDate;

    public FileLogger(string? logDirectory = null)
    {
        _logDirectory = logDirectory ?? Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
        _currentDate = DateTime.UtcNow.Date;
        _writer = CreateWriterForDate(_currentDate);
    }

    public void Info(string message)  => Write("INFO", message);
    public void Warn(string message)  => Write("WARN", message);
    public void Debug(string message) => Write("DEBUG", message);
    public void Error(string message, Exception? ex = null)
    {
        var full = ex is null ? message : $"{message}{Environment.NewLine}{ex}";
        Write("ERROR", full);
    }

    private StreamWriter CreateWriterForDate(DateTime date)
    {
        var file = Path.Combine(_logDirectory, $"sms-{date:yyyy-MM-dd}.log");
        var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        return new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
    }

    private void Write(string level, string message)
    {
        lock (_lock)
        {
            var today = DateTime.UtcNow.Date;
            if (today != _currentDate)
            {
                _writer?.Dispose();
                _writer = CreateWriterForDate(today);
                _currentDate = today;
            }
            var line = $"{DateTime.UtcNow:O} [{level}] {message}";
            _writer!.WriteLine(line);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }
}
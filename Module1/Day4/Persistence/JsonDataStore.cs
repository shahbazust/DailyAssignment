using System.Text.Json;
using StudentManagement.Domain;

namespace StudentManagement.Persistence;

public sealed class JsonDataStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonDataStore(string? dataDir = null)
    {
        dataDir ??= Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "sms.json");
    }

    public async Task SaveAsync(DataSnapshot snapshot, CancellationToken ct = default)
    {
        await using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, snapshot, _options, ct);
    }

    public async Task<DataSnapshot> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath)) return new DataSnapshot();
        await using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var snap = await JsonSerializer.DeserializeAsync<DataSnapshot>(fs, _options, ct);
        return snap ?? new DataSnapshot();
    }
}

// public sealed class DataSnapshot
// {
//     public List<Student> Students { get; set; } = new();
//     public List<Course> Courses { get; set; } = new();
//     public List<Enrollment> Enrollments { get; set; } = new();
// }


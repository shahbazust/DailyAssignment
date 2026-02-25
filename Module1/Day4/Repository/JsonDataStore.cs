using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StudentManagement.Domain;

namespace StudentManagement.Repository
{
  
    public sealed class JsonDataStore : IJsonDataStore
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JsonDataStore(string? filePath = null, bool ensureDirectory = false)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? Path.Combine(AppContext.BaseDirectory, "sms.json")
                : (Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(filePath));

            if (ensureDirectory)
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        public async Task SaveAsync(DataSnapshot snapshot)
        {
            if (snapshot == null) return;

            await _gate.WaitAsync();
            try
            {
                string json = JsonSerializer.Serialize(snapshot, Options);
                string tmp = _filePath + ".tmp";
                await File.WriteAllTextAsync(tmp, json, Encoding.UTF8);
                File.Copy(tmp, _filePath, overwrite: true);
                File.Delete(tmp);
            }
            finally { _gate.Release(); }
        }

        public async Task<DataSnapshot> LoadAsync()
        {
            await _gate.WaitAsync();
            try
            {
                if (!File.Exists(_filePath)) return new DataSnapshot();
                string json = await File.ReadAllTextAsync(_filePath, Encoding.UTF8);
                var snap = JsonSerializer.Deserialize<DataSnapshot>(json, Options);
                return snap ?? new DataSnapshot();
            }
            finally { _gate.Release(); }
        }

    
        public async Task<TResult> ReadAsync<TResult>(Func<DataSnapshot, TResult> selector)
        {
            await _gate.WaitAsync();
            try
            {
                var snap = await LoadAsync();
                return selector(snap);
            }
            finally { _gate.Release(); }
        }

        public async Task<TResult> UpdateAsync<TResult>(Func<DataSnapshot, TResult> mutator)
        {
            await _gate.WaitAsync();
            try
            {
                var snap = await LoadAsync();
                var result = mutator(snap);
                await SaveAsync(snap);
                return result;
            }
            finally { _gate.Release(); }
        }
    }
}
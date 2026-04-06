using System.Text.Json;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public abstract class JsonRepositoryBase<T>(StoragePaths storagePaths, string fileName)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _filePath = storagePaths.GetFilePath(fileName);

    public async Task<List<T>> GetAllAsync()
    {
        if (!File.Exists(_filePath))
        {
            await SaveAllAsync([]);
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? [];
    }

    public async Task SaveAllAsync(List<T> items)
    {
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}

using System.Text.Json;
using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Services;

/// <summary>
/// Lightweight read-through cache for holons. Persists to disk so offline reads
/// work after the circuit breaker opens or the API is unreachable.
/// </summary>
public class HolonCacheService
{
    private readonly string _cacheDir;
    private readonly Dictionary<string, List<Holon>> _memory = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public HolonCacheService()
    {
        _cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OasisHyperDriveClient", "cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public void Store(string key, List<Holon> holons)
    {
        _memory[key] = holons;
        var path = CachePath(key);
        File.WriteAllText(path, JsonSerializer.Serialize(holons, JsonOpts));
    }

    public List<Holon>? TryGet(string key)
    {
        if (_memory.TryGetValue(key, out var cached)) return cached;

        var path = CachePath(key);
        if (!File.Exists(path)) return null;

        try
        {
            var holons = JsonSerializer.Deserialize<List<Holon>>(File.ReadAllText(path), JsonOpts);
            if (holons is not null) _memory[key] = holons;
            return holons;
        }
        catch { return null; }
    }

    public void Invalidate(string key)
    {
        _memory.Remove(key);
        var path = CachePath(key);
        if (File.Exists(path)) File.Delete(path);
    }

    public void InvalidateAll()
    {
        _memory.Clear();
        foreach (var f in Directory.GetFiles(_cacheDir, "*.json"))
            File.Delete(f);
    }

    public bool HasOfflineData(string key) =>
        _memory.ContainsKey(key) || File.Exists(CachePath(key));

    private string CachePath(string key) =>
        Path.Combine(_cacheDir, $"{SanitiseKey(key)}.json");

    private static string SanitiseKey(string key) =>
        string.Concat(key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}

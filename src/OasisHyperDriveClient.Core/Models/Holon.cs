namespace OasisHyperDriveClient.Core.Models;

public class Holon
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HolonType { get; set; } = string.Empty;
    public string? CustomKey { get; set; }
    public bool IsNewHolon { get; set; }
    public bool IsSaving { get; set; }
    public int Version { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public GlobalHolonData? GlobalHolonData { get; set; }
    public Dictionary<string, string> ProviderUniqueStorageKey { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>> ProviderMetaData { get; set; } = new();
    public Guid? ParentZomeId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? CreatedByAvatarId { get; set; }
    public string? ModifiedByAvatarId { get; set; }
}

public class GlobalHolonData
{
    public Dictionary<string, object> Data { get; set; } = new();
}

public class HolonViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string HolonType { get; init; } = string.Empty;
    public string? PrimaryProvider { get; init; }
    public long? SizeBytes { get; init; }
    public DateTime? Modified { get; init; }
    public DateTime? Created { get; init; }
    public Dictionary<string, string> ProviderKeys { get; init; } = new();
    public bool HasChildren { get; init; }
    public IReadOnlyList<string> ReplicatedProviders { get; init; } = [];

    public string DisplayIcon => HolonType switch
    {
        "File" => "📄",
        "NFT" => "🖼",
        "GeoNFT" => "🌍",
        "Avatar" => "💠",
        "Keys" => "🔑",
        _ => "🔷"
    };

    public string SizeDisplay => SizeBytes.HasValue
        ? SizeBytes.Value switch
        {
            < 1024 => $"{SizeBytes} B",
            < 1024 * 1024 => $"{SizeBytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{SizeBytes / (1024.0 * 1024):F1} MB",
            _ => $"{SizeBytes / (1024.0 * 1024 * 1024):F1} GB"
        }
        : "—";

    public static HolonViewModel FromHolon(Holon h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        HolonType = string.IsNullOrEmpty(h.HolonType) ? "Holon" : h.HolonType,
        PrimaryProvider = h.ProviderUniqueStorageKey.Keys.FirstOrDefault(),
        Modified = h.ModifiedDate,
        Created = h.CreatedDate,
        ProviderKeys = h.ProviderUniqueStorageKey,
        ReplicatedProviders = [.. h.ProviderUniqueStorageKey.Keys]
    };
}

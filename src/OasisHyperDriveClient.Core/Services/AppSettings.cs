using System.Text.Json;

namespace OasisHyperDriveClient.Core.Services;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://api.oasis.ac";
    public string DefaultProvider { get; set; } = string.Empty;
    public bool AutoStartOnLogin { get; set; } = false;
    public string Theme { get; set; } = "Dark";
    public int DashboardRefreshSeconds { get; set; } = 30;

    public NotificationSettings Notifications { get; set; } = new();

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OasisHyperDriveClient",
        "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new();
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* silently ignore */ }
    }
}

public class NotificationSettings
{
    public bool FailoverTriggered { get; set; } = true;
    public bool ProviderDown { get; set; } = true;
    public bool ReplicationComplete { get; set; } = false;
    public bool FileReceived { get; set; } = true;
    public bool UploadComplete { get; set; } = false;
    public int QuotaWarningPercent { get; set; } = 80;
}

namespace OasisHyperDriveClient.Core.Auth;

/// <summary>
/// Simple file-based credential store as a fallback.
/// Windows/macOS/Linux platform implementations should override with OS keychain.
/// </summary>
public class FileCredentialStore : ICredentialStore
{
    private readonly string _tokenPath;

    public FileCredentialStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OasisHyperDriveClient");
        Directory.CreateDirectory(dir);
        _tokenPath = Path.Combine(dir, ".session");
    }

    public void SaveToken(string token)
    {
        // Obfuscate slightly — proper OS keychain is used in platform-specific code
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
        File.WriteAllText(_tokenPath, encoded);
    }

    public string? LoadToken()
    {
        if (!File.Exists(_tokenPath)) return null;
        try
        {
            var encoded = File.ReadAllText(_tokenPath);
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            return null;
        }
    }

    public void ClearToken()
    {
        if (File.Exists(_tokenPath))
            File.Delete(_tokenPath);
    }
}

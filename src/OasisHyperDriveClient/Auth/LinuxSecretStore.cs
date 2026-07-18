using System.Diagnostics;
using OasisHyperDriveClient.Core.Auth;

namespace OasisHyperDriveClient.Auth;

public class LinuxSecretStore : ICredentialStore
{
    private const string Schema = "oasis.hyperdrive.client";
    private const string Label = "OASIS HyperDrive JWT";

    public void SaveToken(string token)
    {
        // Uses secret-tool (libsecret CLI) — falls back to FileCredentialStore if unavailable
        if (!SecretToolAvailable()) { _fallback.SaveToken(token); return; }
        Run("secret-tool", $"store --label=\"{Label}\" app {Schema} key jwt", token);
    }

    public string? LoadToken()
    {
        if (!SecretToolAvailable()) return _fallback.LoadToken();
        var result = Run("secret-tool", $"lookup app {Schema} key jwt");
        return string.IsNullOrWhiteSpace(result) ? _fallback.LoadToken() : result.Trim();
    }

    public void ClearToken()
    {
        if (SecretToolAvailable())
            Run("secret-tool", $"clear app {Schema} key jwt");
        _fallback.ClearToken();
    }

    private readonly OasisHyperDriveClient.Core.Auth.FileCredentialStore _fallback = new();

    private static bool SecretToolAvailable()
    {
        try { return Process.Start(new ProcessStartInfo("which", "secret-tool") { RedirectStandardOutput = true, UseShellExecute = false })?.WaitForExit(2000) == true; }
        catch { return false; }
    }

    private static string Run(string cmd, string args, string? stdin = null)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = stdin is not null,
                UseShellExecute = false
            };
            using var p = Process.Start(psi)!;
            if (stdin is not null) { p.StandardInput.WriteLine(stdin); p.StandardInput.Close(); }
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }
        catch { return string.Empty; }
    }
}

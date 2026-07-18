using System.Diagnostics;
using System.Runtime.Versioning;
using OasisHyperDriveClient.Core.Auth;

namespace OasisHyperDriveClient.Auth;

[SupportedOSPlatform("macos")]
public class MacKeychainStore : ICredentialStore
{
    private const string Service = "OASISHyperDriveClient";
    private const string Account = "jwt";

    public void SaveToken(string token)
    {
        // Delete existing first (security add-generic-password will error if it exists)
        Run("security", $"delete-generic-password -s \"{Service}\" -a \"{Account}\"");
        Run("security", $"add-generic-password -s \"{Service}\" -a \"{Account}\" -w \"{Escape(token)}\"");
    }

    public string? LoadToken()
    {
        var result = Run("security", $"find-generic-password -s \"{Service}\" -a \"{Account}\" -w");
        return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
    }

    public void ClearToken() =>
        Run("security", $"delete-generic-password -s \"{Service}\" -a \"{Account}\"");

    private static string Run(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi)!;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }
        catch { return string.Empty; }
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

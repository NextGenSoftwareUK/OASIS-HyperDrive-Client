using System.Runtime.Versioning;
using Microsoft.Win32;
using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Services;

[SupportedOSPlatform("windows")]
public class WindowsAutoStartService : IAutoStartService
{
    private const string AppName = "OasisHyperDriveClient";
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
                return key?.GetValue(AppName) is not null;
            }
            catch { return false; }
        }
    }

    public void Enable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            key?.SetValue(AppName, $"\"{Environment.ProcessPath}\"");
        }
        catch { /* silently ignore */ }
    }

    public void Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
        catch { /* silently ignore */ }
    }
}

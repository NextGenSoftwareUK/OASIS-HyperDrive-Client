using System.Runtime.Versioning;
using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Services;

[SupportedOSPlatform("linux")]
public class LinuxAutoStartService : IAutoStartService
{
    private static readonly string DesktopFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "autostart", "oasis-hyperdrive.desktop");

    public bool IsEnabled => File.Exists(DesktopFilePath);

    public void Enable()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DesktopFilePath)!);
            File.WriteAllText(DesktopFilePath,
                $"""
                [Desktop Entry]
                Type=Application
                Name=OASIS HyperDrive
                Exec={Environment.ProcessPath}
                Hidden=false
                NoDisplay=false
                X-GNOME-Autostart-enabled=true
                """);
        }
        catch { /* silently ignore */ }
    }

    public void Disable()
    {
        try { File.Delete(DesktopFilePath); }
        catch { /* silently ignore */ }
    }
}

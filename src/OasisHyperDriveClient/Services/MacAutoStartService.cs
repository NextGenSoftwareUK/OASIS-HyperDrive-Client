using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Services;

public class MacAutoStartService : IAutoStartService
{
    private static readonly string PlistPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library", "LaunchAgents", "com.oasis.hyperdrive.plist");

    public bool IsEnabled => File.Exists(PlistPath);

    public void Enable()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlistPath)!);
            var exec = Environment.ProcessPath ?? string.Empty;
            File.WriteAllText(PlistPath,
                $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                <plist version="1.0">
                <dict>
                    <key>Label</key>
                    <string>com.oasis.hyperdrive</string>
                    <key>ProgramArguments</key>
                    <array>
                        <string>{exec}</string>
                    </array>
                    <key>RunAtLoad</key>
                    <true/>
                </dict>
                </plist>
                """);
        }
        catch { /* silently ignore */ }
    }

    public void Disable()
    {
        try { File.Delete(PlistPath); }
        catch { /* silently ignore */ }
    }
}

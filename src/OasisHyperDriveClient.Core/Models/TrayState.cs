namespace OasisHyperDriveClient.Core.Models;

public enum TrayState
{
    Disabled,
    Connecting,
    Healthy,
    Degraded,
    Error,
    Syncing,
    Busy
}

public class TrayStateInfo
{
    public TrayState State { get; init; }
    public string Message { get; init; } = string.Empty;
    public int ActiveProviders { get; init; }
    public int TotalProviders { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

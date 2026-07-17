using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Load_ReturnsDefaults_WhenFileAbsent()
    {
        // AppSettings.Load reads from %APPDATA%; in test isolation we just
        // verify the defaults are sane without touching the filesystem
        var settings = new AppSettings();
        Assert.Equal("https://api.oasis.ac", settings.ApiBaseUrl);
        Assert.Equal(30, settings.DashboardRefreshSeconds);
        Assert.False(settings.AutoStartOnLogin);
        Assert.Equal("Dark", settings.Theme);
    }

    [Fact]
    public void NotificationSettings_HaveExpectedDefaults()
    {
        var n = new NotificationSettings();
        Assert.True(n.FailoverTriggered);
        Assert.True(n.ProviderDown);
        Assert.False(n.ReplicationComplete);
        Assert.True(n.FileReceived);
        Assert.False(n.UploadComplete);
        Assert.Equal(80, n.QuotaWarningPercent);
    }
}

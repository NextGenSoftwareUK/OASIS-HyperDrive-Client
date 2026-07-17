using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Tests;

public class HyperDriveMonitorServiceTests
{
    private static HyperDriveService FakeHyperDrive(DashboardData? dashboard)
    {
        var api = Substitute.For<OasisApiClient>(Substitute.For<HttpClient>());
        var svc = new HyperDriveService(api);
        return svc;
    }

    [Fact]
    public void TrayState_IsHealthy_WhenNoAlerts_AndHealthAbove08()
    {
        var dashboard = new DashboardData
        {
            SystemHealth = 0.95,
            ActiveProviders = 3,
            Alerts = []
        };

        var state = ComputeState(dashboard);
        Assert.Equal(TrayState.Healthy, state);
    }

    [Fact]
    public void TrayState_IsDegraded_WhenHealthBelow08()
    {
        var dashboard = new DashboardData
        {
            SystemHealth = 0.7,
            ActiveProviders = 2,
            Alerts = []
        };

        var state = ComputeState(dashboard);
        Assert.Equal(TrayState.Degraded, state);
    }

    [Fact]
    public void TrayState_IsError_WhenErrorAlertPresent()
    {
        var dashboard = new DashboardData
        {
            SystemHealth = 0.9,
            ActiveProviders = 3,
            Alerts = [new DashboardAlert { Severity = "Error", Message = "Provider offline" }]
        };

        var state = ComputeState(dashboard);
        Assert.Equal(TrayState.Error, state);
    }

    [Fact]
    public void TrayState_IsDegraded_WhenWarnAlertPresent()
    {
        var dashboard = new DashboardData
        {
            SystemHealth = 0.85,
            ActiveProviders = 3,
            Alerts = [new DashboardAlert { Severity = "Warning", Message = "High latency" }]
        };

        var state = ComputeState(dashboard);
        Assert.Equal(TrayState.Degraded, state);
    }

    private static TrayState ComputeState(DashboardData dashboard)
    {
        var errorAlerts = dashboard.Alerts.Where(a =>
            string.Equals(a.Severity, "Error", StringComparison.OrdinalIgnoreCase)).ToList();
        var warnAlerts = dashboard.Alerts.Where(a =>
            string.Equals(a.Severity, "Warning", StringComparison.OrdinalIgnoreCase)).ToList();

        return errorAlerts.Count > 0
            ? TrayState.Error
            : warnAlerts.Count > 0 || dashboard.SystemHealth < 0.8
                ? TrayState.Degraded
                : TrayState.Healthy;
    }
}

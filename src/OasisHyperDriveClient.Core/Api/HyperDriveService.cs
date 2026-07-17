using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Api;

public class HyperDriveService
{
    private readonly OasisApiClient _api;

    public HyperDriveService(OasisApiClient api)
    {
        _api = api;
    }

    public async Task<DashboardData?> GetDashboardAsync(CancellationToken ct = default)
    {
        var result = await _api.GetAsync<DashboardData>("api/hyperDrive/dashboard", ct);
        return result.IsError ? null : result.Result;
    }

    public async Task<Dictionary<string, ProviderPerformanceMetrics>?> GetMetricsAsync(CancellationToken ct = default)
    {
        var result = await _api.GetAsync<Dictionary<string, ProviderPerformanceMetrics>>("api/hyperDrive/metrics", ct);
        return result.IsError ? null : result.Result;
    }

    public async Task<HyperDriveConfig?> GetConfigurationAsync(CancellationToken ct = default)
    {
        var result = await _api.GetAsync<HyperDriveConfig>("api/hyperDrive/config", ct);
        return result.IsError ? null : result.Result;
    }

    public async Task<bool> EnableIntelligentModeAsync(CancellationToken ct = default)
    {
        var result = await _api.PostAsync<bool>("api/hyperDrive/intelligent-mode/enable", null, ct);
        return !result.IsError && result.Result;
    }

    public async Task<bool> DisableIntelligentModeAsync(CancellationToken ct = default)
    {
        var result = await _api.PostAsync<bool>("api/hyperDrive/intelligent-mode/disable", null, ct);
        return !result.IsError && result.Result;
    }
}

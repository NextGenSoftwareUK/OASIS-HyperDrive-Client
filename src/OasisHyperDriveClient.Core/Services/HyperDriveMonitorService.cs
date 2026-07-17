using Microsoft.Extensions.Hosting;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Services;

public class HyperDriveMonitorService : BackgroundService
{
    private readonly HyperDriveService _hyperDrive;
    private readonly TimeSpan _interval;

    private TrayStateInfo _current = new() { State = TrayState.Disabled };

    public event EventHandler<TrayStateInfo>? StateChanged;
    public event EventHandler<DashboardAlert>? AlertReceived;

    public TrayStateInfo CurrentState => _current;

    public HyperDriveMonitorService(HyperDriveService hyperDrive, TimeSpan? interval = null)
    {
        _hyperDrive = hyperDrive;
        _interval = interval ?? TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
        }
    }

    public async Task PollAsync(CancellationToken ct = default)
    {
        try
        {
            var dashboard = await _hyperDrive.GetDashboardAsync(ct);
            if (dashboard is null)
            {
                SetState(new TrayStateInfo
                {
                    State = TrayState.Disabled,
                    Message = "Cannot reach OASIS API"
                });
                return;
            }

            var errorAlerts = dashboard.Alerts.Where(a =>
                string.Equals(a.Severity, "Error", StringComparison.OrdinalIgnoreCase)).ToList();
            var warnAlerts = dashboard.Alerts.Where(a =>
                string.Equals(a.Severity, "Warning", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var alert in dashboard.Alerts)
                AlertReceived?.Invoke(this, alert);

            var state = errorAlerts.Count > 0
                ? TrayState.Error
                : warnAlerts.Count > 0 || dashboard.SystemHealth < 0.8
                    ? TrayState.Degraded
                    : TrayState.Healthy;

            SetState(new TrayStateInfo
            {
                State = state,
                ActiveProviders = dashboard.ActiveProviders,
                WarningCount = warnAlerts.Count,
                ErrorCount = errorAlerts.Count,
                Message = state == TrayState.Healthy
                    ? $"{dashboard.ActiveProviders} providers active"
                    : errorAlerts.FirstOrDefault()?.Message ?? warnAlerts.FirstOrDefault()?.Message ?? "",
                LastUpdated = DateTime.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch
        {
            SetState(new TrayStateInfo
            {
                State = TrayState.Disabled,
                Message = "Connection error"
            });
        }
    }

    private void SetState(TrayStateInfo info)
    {
        _current = info;
        StateChanged?.Invoke(this, info);
    }
}

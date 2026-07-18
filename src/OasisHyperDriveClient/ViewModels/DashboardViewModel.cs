using System.Collections.ObjectModel;
using System.Windows.Input;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly HyperDriveService _hyperDrive;
    private readonly HyperDriveMonitorService _monitor;

    private double _systemHealth;
    private int _activeProviders;
    private int _totalRequests;
    private double _avgResponseTime;
    private double _totalCost;
    private bool _isLoading;
    private string _lastUpdated = "Never";

    public double SystemHealth
    {
        get => _systemHealth;
        set => this.RaiseAndSetIfChanged(ref _systemHealth, value);
    }

    public int ActiveProviders
    {
        get => _activeProviders;
        set => this.RaiseAndSetIfChanged(ref _activeProviders, value);
    }

    public int TotalRequests
    {
        get => _totalRequests;
        set => this.RaiseAndSetIfChanged(ref _totalRequests, value);
    }

    public double AvgResponseTime
    {
        get => _avgResponseTime;
        set => this.RaiseAndSetIfChanged(ref _avgResponseTime, value);
    }

    public double TotalCost
    {
        get => _totalCost;
        set => this.RaiseAndSetIfChanged(ref _totalCost, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string LastUpdated
    {
        get => _lastUpdated;
        set => this.RaiseAndSetIfChanged(ref _lastUpdated, value);
    }

    public string SystemHealthDisplay => $"{SystemHealth * 100:F0}%";

    public ObservableCollection<ProviderPerformanceMetrics> ProviderMetrics { get; } = [];
    public ObservableCollection<DashboardAlert> Alerts { get; } = [];
    public ObservableCollection<HyperDriveRecommendation> Recommendations { get; } = [];

    public ICommand RefreshCommand { get; }

    public DashboardViewModel(HyperDriveService hyperDrive, HyperDriveMonitorService monitor)
    {
        _hyperDrive = hyperDrive;
        _monitor = monitor;
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        _monitor.StateChanged += (_, _) => _ = LoadAsync();
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var dashboard = await _hyperDrive.GetDashboardAsync(ct);
            if (dashboard is null) return;

            SystemHealth = dashboard.SystemHealth;
            ActiveProviders = dashboard.ActiveProviders;
            TotalRequests = dashboard.TotalRequests;
            AvgResponseTime = dashboard.PerformanceMetrics?.AverageResponseTime ?? 0;
            TotalCost = dashboard.CostMetrics?.TotalCost ?? 0;
            LastUpdated = DateTime.Now.ToString("HH:mm:ss");
            this.RaisePropertyChanged(nameof(SystemHealthDisplay));

            Alerts.Clear();
            foreach (var a in dashboard.Alerts)
                Alerts.Add(a);

            var metrics = await _hyperDrive.GetMetricsAsync(ct);
            ProviderMetrics.Clear();
            if (metrics is not null)
                foreach (var m in metrics.Values)
                    ProviderMetrics.Add(m);

            var recs = await _hyperDrive.GetRecommendationsAsync(ct);
            Recommendations.Clear();
            foreach (var r in recs)
                Recommendations.Add(r);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

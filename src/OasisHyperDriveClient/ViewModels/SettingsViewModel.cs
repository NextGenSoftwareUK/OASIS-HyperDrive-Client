using System.Reactive;
using OasisHyperDriveClient.Core.Services;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    private readonly AppSettings _settings;
    private readonly IAutoStartService _autoStart;

    private string _apiBaseUrl = string.Empty;
    private string _defaultProvider = string.Empty;
    private bool _autoStartOnLogin;
    private string _theme = "Dark";
    private int _dashboardRefreshSeconds = 30;
    private bool _notifyFailover;
    private bool _notifyProviderDown;
    private bool _notifyReplication;
    private bool _notifyFileReceived;
    private bool _notifyUploadComplete;
    private int _quotaWarningPercent = 90;

    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => this.RaiseAndSetIfChanged(ref _apiBaseUrl, value);
    }
    public string DefaultProvider
    {
        get => _defaultProvider;
        set => this.RaiseAndSetIfChanged(ref _defaultProvider, value);
    }
    public bool AutoStartOnLogin
    {
        get => _autoStartOnLogin;
        set => this.RaiseAndSetIfChanged(ref _autoStartOnLogin, value);
    }
    public string Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }
    public int DashboardRefreshSeconds
    {
        get => _dashboardRefreshSeconds;
        set => this.RaiseAndSetIfChanged(ref _dashboardRefreshSeconds, value);
    }
    public bool NotifyFailover
    {
        get => _notifyFailover;
        set => this.RaiseAndSetIfChanged(ref _notifyFailover, value);
    }
    public bool NotifyProviderDown
    {
        get => _notifyProviderDown;
        set => this.RaiseAndSetIfChanged(ref _notifyProviderDown, value);
    }
    public bool NotifyReplication
    {
        get => _notifyReplication;
        set => this.RaiseAndSetIfChanged(ref _notifyReplication, value);
    }
    public bool NotifyFileReceived
    {
        get => _notifyFileReceived;
        set => this.RaiseAndSetIfChanged(ref _notifyFileReceived, value);
    }
    public bool NotifyUploadComplete
    {
        get => _notifyUploadComplete;
        set => this.RaiseAndSetIfChanged(ref _notifyUploadComplete, value);
    }
    public int QuotaWarningPercent
    {
        get => _quotaWarningPercent;
        set => this.RaiseAndSetIfChanged(ref _quotaWarningPercent, value);
    }

    public ReactiveCommand<Unit, Unit> SaveCommand   { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public SettingsViewModel(AppSettings settings, IAutoStartService autoStart)
    {
        _settings = settings;
        _autoStart = autoStart;

        ApiBaseUrl             = settings.ApiBaseUrl;
        DefaultProvider        = settings.DefaultProvider;
        AutoStartOnLogin       = settings.AutoStartOnLogin;
        Theme                  = settings.Theme;
        DashboardRefreshSeconds = settings.DashboardRefreshSeconds;
        NotifyFailover         = settings.Notifications.FailoverTriggered;
        NotifyProviderDown     = settings.Notifications.ProviderDown;
        NotifyReplication      = settings.Notifications.ReplicationComplete;
        NotifyFileReceived     = settings.Notifications.FileReceived;
        NotifyUploadComplete   = settings.Notifications.UploadComplete;
        QuotaWarningPercent    = settings.Notifications.QuotaWarningPercent;

        SaveCommand   = ReactiveCommand.Create(DoSave);
        CancelCommand = ReactiveCommand.Create(() => Cancelled?.Invoke(this, EventArgs.Empty));
    }

    private void DoSave()
    {
        _settings.ApiBaseUrl              = ApiBaseUrl;
        _settings.DefaultProvider         = DefaultProvider;
        _settings.AutoStartOnLogin        = AutoStartOnLogin;
        _settings.Theme                   = Theme;
        _settings.DashboardRefreshSeconds = DashboardRefreshSeconds;
        _settings.Notifications.FailoverTriggered   = NotifyFailover;
        _settings.Notifications.ProviderDown        = NotifyProviderDown;
        _settings.Notifications.ReplicationComplete = NotifyReplication;
        _settings.Notifications.FileReceived        = NotifyFileReceived;
        _settings.Notifications.UploadComplete      = NotifyUploadComplete;
        _settings.Notifications.QuotaWarningPercent = QuotaWarningPercent;
        _settings.Save();

        if (AutoStartOnLogin) _autoStart.Enable();
        else _autoStart.Disable();

        Saved?.Invoke(this, EventArgs.Empty);
    }
}

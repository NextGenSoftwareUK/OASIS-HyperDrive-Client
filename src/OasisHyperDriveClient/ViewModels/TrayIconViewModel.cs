using System.Windows.Input;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class TrayIconViewModel : ViewModelBase
{
    private readonly HyperDriveMonitorService _monitor;

    private TrayStateInfo _stateInfo = new() { State = TrayState.Disabled };
    private string _iconPath = "avares://OasisHyperDriveClient/Assets/Icons/tray-disabled.png";
    private string _toolTipText = "OASIS HyperDrive — Connecting...";

    public string IconPath
    {
        get => _iconPath;
        private set => this.RaiseAndSetIfChanged(ref _iconPath, value);
    }

    public string ToolTipText
    {
        get => _toolTipText;
        private set => this.RaiseAndSetIfChanged(ref _toolTipText, value);
    }

    public TrayStateInfo StateInfo
    {
        get => _stateInfo;
        private set => this.RaiseAndSetIfChanged(ref _stateInfo, value);
    }

    public TrayState CurrentState => _stateInfo.State;

    public ICommand OpenBrowserCommand { get; }
    public ICommand OpenDashboardCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand QuitCommand { get; }

    public TrayIconViewModel(
        HyperDriveMonitorService monitor,
        Action openBrowser,
        Action openDashboard,
        Action openSettings,
        Action signOut,
        Action quit)
    {
        _monitor = monitor;
        _monitor.StateChanged += OnStateChanged;

        OpenBrowserCommand = ReactiveCommand.Create(openBrowser);
        OpenDashboardCommand = ReactiveCommand.Create(openDashboard);
        OpenSettingsCommand = ReactiveCommand.Create(openSettings);
        SignOutCommand = ReactiveCommand.Create(signOut);
        QuitCommand = ReactiveCommand.Create(quit);
    }

    private void OnStateChanged(object? sender, TrayStateInfo info)
    {
        StateInfo = info;
        this.RaisePropertyChanged(nameof(CurrentState));
        IconPath = info.State switch
        {
            TrayState.Healthy => "avares://OasisHyperDriveClient/Assets/Icons/tray-healthy.png",
            TrayState.Degraded => "avares://OasisHyperDriveClient/Assets/Icons/tray-warning.png",
            TrayState.Error => "avares://OasisHyperDriveClient/Assets/Icons/tray-error.png",
            TrayState.Syncing => "avares://OasisHyperDriveClient/Assets/Icons/tray-syncing.png",
            TrayState.Busy => "avares://OasisHyperDriveClient/Assets/Icons/tray-busy.png",
            TrayState.Connecting => "avares://OasisHyperDriveClient/Assets/Icons/tray-connecting.png",
            _ => "avares://OasisHyperDriveClient/Assets/Icons/tray-disabled.png"
        };

        var providerLine = info.ActiveProviders > 0
            ? $"● {info.ActiveProviders} providers active"
            : "● No providers connected";

        var alertLine = info.ErrorCount > 0
            ? $"  ✕ {info.ErrorCount} error(s)"
            : info.WarningCount > 0
                ? $"  ▲ {info.WarningCount} warning(s)"
                : "  ✓ No alerts";

        ToolTipText = $"OASIS HyperDrive\n{providerLine}{alertLine}";
    }
}

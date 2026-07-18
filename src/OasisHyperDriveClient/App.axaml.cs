using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Auth;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;
using OasisHyperDriveClient.Services;
using OasisHyperDriveClient.ViewModels;
using OasisHyperDriveClient.Views;
using ReactiveUI;

namespace OasisHyperDriveClient;

public partial class App : Application
{
    private ServiceProvider? _services;
    private FileBrowserWindow? _browserWindow;
    private TrayIcon? _trayIcon;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _services = BuildServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SetupTrayIcon();
            _ = StartAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ServiceProvider BuildServices()
    {
        var settings = AppSettings.Load();

        var services = new ServiceCollection();

        var apiBaseUrl = Environment.GetEnvironmentVariable("OASIS_API_URL")
            ?? settings.ApiBaseUrl;

        services.AddHttpClient("oasis", c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<OasisApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient("oasis");
            return new OasisApiClient(http);
        });

        services.AddSingleton(settings);
        services.AddSingleton<AvatarService>();
        services.AddSingleton<DataService>();
        services.AddSingleton<HyperDriveService>();
#pragma warning disable CA1416 // OS guards immediately above guarantee correct platform
        if (OperatingSystem.IsWindows())
            services.AddSingleton<ICredentialStore, Auth.WindowsCredentialStore>();
        else if (OperatingSystem.IsMacOS())
            services.AddSingleton<ICredentialStore, Auth.MacKeychainStore>();
        else
            services.AddSingleton<ICredentialStore, Auth.LinuxSecretStore>();
#pragma warning restore CA1416
        services.AddSingleton<AuthService>();
        services.AddSingleton<HyperDriveMonitorService>(sp =>
            new HyperDriveMonitorService(
                sp.GetRequiredService<HyperDriveService>(),
                TimeSpan.FromSeconds(sp.GetRequiredService<AppSettings>().DashboardRefreshSeconds)));
        services.AddSingleton<HolonCacheService>();
        services.AddSingleton<INotificationService, AvaloniaNotificationService>();

#pragma warning disable CA1416 // OS guards immediately above guarantee correct platform
        if (OperatingSystem.IsWindows())
            services.AddSingleton<IAutoStartService, WindowsAutoStartService>();
        else if (OperatingSystem.IsMacOS())
            services.AddSingleton<IAutoStartService, MacAutoStartService>();
        else
            services.AddSingleton<IAutoStartService, LinuxAutoStartService>();
#pragma warning restore CA1416

        return services.BuildServiceProvider();
    }

    private void SetupTrayIcon()
    {
        var monitor = _services!.GetRequiredService<HyperDriveMonitorService>();
        var trayVm = new TrayIconViewModel(
            monitor,
            openBrowser: OpenBrowser,
            openDashboard: OpenDashboard,
            openSettings: OpenSettings,
            signOut: SignOut,
            quit: Quit);

        DataContext = trayVm;

        _trayIcon = new TrayIcon
        {
            ToolTipText = "OASIS HyperDrive",
            IsVisible = true,
            Menu = (NativeMenu?)NativeMenu.GetMenu(this)
        };

        trayVm.WhenAnyValue(x => x.ToolTipText)
            .Subscribe(t => _trayIcon.ToolTipText = t);

        trayVm.WhenAnyValue(x => x.CurrentState)
            .Subscribe(state =>
            {
                try
                {
                    using var stream = TrayIconRenderer.Render(state);
                    _trayIcon.Icon = new WindowIcon(stream);
                }
                catch { /* fall through — no icon update */ }
            });

        _trayIcon.Clicked += (_, _) => OpenBrowser();
    }

    private async Task StartAsync()
    {
        var auth = _services!.GetRequiredService<AuthService>();
        var monitor = _services!.GetRequiredService<HyperDriveMonitorService>();

        var hasSession = await auth.TryRestoreSessionAsync();
        if (!hasSession)
        {
            var loginVm = new LoginViewModel(auth);
            var loginWin = new LoginWindow(loginVm);

            // No MainWindow in tray-only mode — use Show + TCS instead of ShowDialog
            var tcs = new TaskCompletionSource<bool>();
            loginVm.LoginSucceeded += (_, _) => { tcs.TrySetResult(true); loginWin.Close(); };
            loginWin.Closed += (_, _) => tcs.TrySetResult(false);
            loginWin.Show();
            var loggedIn = await tcs.Task;

            if (!loggedIn)
            {
                Quit();
                return;
            }
        }

        _ = monitor.StartAsync(CancellationToken.None);
    }

    private void OpenBrowser()
    {
        if (_browserWindow is null)
        {
            var vm = new FileBrowserViewModel(
                _services!.GetRequiredService<DataService>(),
                _services!.GetRequiredService<HyperDriveService>(),
                _services!.GetRequiredService<AuthService>(),
                _services!.GetRequiredService<HyperDriveMonitorService>());

            _browserWindow = new FileBrowserWindow(
                vm,
                _services!.GetRequiredService<DataService>(),
                _services!.GetRequiredService<AvatarService>(),
                _services!.GetRequiredService<INotificationService>(),
                _services!.GetRequiredService<HyperDriveService>());
        }

        _browserWindow.Show();
        _browserWindow.Activate();
    }

    private void OpenDashboard()
    {
        var vm = new DashboardViewModel(
            _services!.GetRequiredService<HyperDriveService>(),
            _services!.GetRequiredService<HyperDriveMonitorService>());
        new DashboardWindow(vm).Show();
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(
            _services!.GetRequiredService<AppSettings>(),
            _services!.GetRequiredService<IAutoStartService>());
        var win = new SettingsWindow(vm);
        vm.Saved += (_, _) => win.Close();
        vm.Cancelled += (_, _) => win.Close();
        win.Show();
    }

    private async void SignOut()
    {
        _services!.GetRequiredService<AuthService>().Logout();
        _browserWindow?.Close();
        _browserWindow = null;
        await StartAsync();
    }

    private void Quit()
    {
        _trayIcon?.Dispose();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}

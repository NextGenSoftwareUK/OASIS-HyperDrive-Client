using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class SettingsWindow : ReactiveWindow<SettingsViewModel>
{
    public SettingsWindow() { InitializeComponent(); }

    public SettingsWindow(SettingsViewModel vm) : this()
    {
        DataContext = vm;
    }
}

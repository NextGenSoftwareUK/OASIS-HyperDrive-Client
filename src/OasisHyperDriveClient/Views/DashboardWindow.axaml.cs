using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class DashboardWindow : ReactiveWindow<DashboardViewModel>
{
    public DashboardWindow()
    {
        InitializeComponent();
    }

    public DashboardWindow(DashboardViewModel vm) : this()
    {
        DataContext = vm;
        _ = vm.LoadAsync();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}

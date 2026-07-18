using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class VersionHistoryWindow : ReactiveWindow<VersionHistoryViewModel>
{
    public VersionHistoryWindow()
    {
        InitializeComponent();
    }

    public VersionHistoryWindow(VersionHistoryViewModel vm) : this()
    {
        DataContext = vm;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is VersionHistoryViewModel vm)
            await vm.LoadAsync();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}

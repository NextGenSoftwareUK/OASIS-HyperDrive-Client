using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class MetadataWindow : ReactiveWindow<MetadataViewModel>
{
    public MetadataWindow()
    {
        InitializeComponent();
    }

    public MetadataWindow(HolonViewModel holon) : this()
    {
        var vm = new MetadataViewModel();
        vm.Load(holon);
        DataContext = vm;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}

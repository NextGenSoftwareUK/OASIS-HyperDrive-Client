using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class RenameDialog : ReactiveWindow<RenameViewModel>
{
    public RenameDialog() { InitializeComponent(); }

    public RenameDialog(RenameViewModel vm) : this()
    {
        DataContext = vm;
    }
}

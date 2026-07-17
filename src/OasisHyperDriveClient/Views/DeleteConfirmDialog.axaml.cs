using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class DeleteConfirmDialog : ReactiveWindow<DeleteConfirmViewModel>
{
    public DeleteConfirmDialog() { InitializeComponent(); }

    public DeleteConfirmDialog(DeleteConfirmViewModel vm) : this()
    {
        DataContext = vm;
    }
}

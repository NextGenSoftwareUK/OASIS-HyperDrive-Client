using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class SendToAvatarDialog : ReactiveWindow<SendToAvatarViewModel>
{
    public SendToAvatarDialog() { InitializeComponent(); }

    public SendToAvatarDialog(SendToAvatarViewModel vm) : this()
    {
        DataContext = vm;
    }
}

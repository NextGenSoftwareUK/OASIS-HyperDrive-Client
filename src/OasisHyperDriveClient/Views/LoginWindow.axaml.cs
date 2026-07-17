using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OasisHyperDriveClient.ViewModels;
using ReactiveUI;

namespace OasisHyperDriveClient.Views;

public partial class LoginWindow : ReactiveWindow<LoginViewModel>
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginViewModel vm) : this()
    {
        DataContext = vm;
        vm.LoginSucceeded += (_, _) => Close(true);
    }
}

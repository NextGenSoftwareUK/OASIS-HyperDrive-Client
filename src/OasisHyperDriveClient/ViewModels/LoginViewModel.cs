using System.Windows.Input;
using OasisHyperDriveClient.Core.Auth;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _auth;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ICommand LoginCommand { get; }

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;

        LoginCommand = ReactiveCommand.CreateFromTask(DoLoginAsync,
            this.WhenAnyValue(x => x.Email, x => x.Password, x => x.IsLoading,
                (e, p, loading) => !string.IsNullOrWhiteSpace(e) && !string.IsNullOrWhiteSpace(p) && !loading));
    }

    private async Task DoLoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var (success, error) = await _auth.LoginAsync(Email, Password);
            if (success)
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            else
                ErrorMessage = error ?? "Login failed. Please check your credentials.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

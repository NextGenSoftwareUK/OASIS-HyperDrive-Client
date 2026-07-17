using System.Reactive;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class RenameViewModel : ReactiveObject
{
    private string _name = string.Empty;
    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }

    public ReactiveCommand<Unit, string> ConfirmCommand { get; }
    public ReactiveCommand<Unit, Unit>   CancelCommand  { get; }

    public event EventHandler<string>? Confirmed;
    public event EventHandler?         Cancelled;

    public RenameViewModel(string currentName)
    {
        _name = currentName;
        var canConfirm = this.WhenAnyValue(x => x.Name, n => !string.IsNullOrWhiteSpace(n));
        ConfirmCommand = ReactiveCommand.Create<string>(() => Name, canConfirm);
        CancelCommand  = ReactiveCommand.Create(() => { Cancelled?.Invoke(this, EventArgs.Empty); });
        ConfirmCommand.Subscribe(n => Confirmed?.Invoke(this, n));
    }
}

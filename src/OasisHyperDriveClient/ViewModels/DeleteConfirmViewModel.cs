using System.Reactive;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class DeleteConfirmViewModel : ReactiveObject
{
    private bool _softDelete = true;
    public bool SoftDelete { get => _softDelete; set => this.RaiseAndSetIfChanged(ref _softDelete, value); }

    public string ItemName { get; }

    public ReactiveCommand<Unit, bool> ConfirmCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand  { get; }

    public event EventHandler<bool>? Confirmed;
    public event EventHandler?       Cancelled;

    public DeleteConfirmViewModel(string itemName)
    {
        ItemName = itemName;
        ConfirmCommand = ReactiveCommand.Create<bool>(() => SoftDelete);
        CancelCommand  = ReactiveCommand.Create(() => { Cancelled?.Invoke(this, EventArgs.Empty); });
        ConfirmCommand.Subscribe(soft => Confirmed?.Invoke(this, soft));
    }
}

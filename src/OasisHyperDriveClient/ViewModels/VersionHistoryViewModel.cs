using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class VersionHistoryViewModel : ViewModelBase
{
    private readonly HyperDriveService _hyperDrive;
    private readonly DataService _data;
    private bool _isLoading;
    private string _statusText = "Loading version history...";
    private HolonVersionViewModel? _selectedVersion;

    public string HolonName { get; }
    public Guid HolonId { get; }
    public ObservableCollection<HolonVersionViewModel> Versions { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public HolonVersionViewModel? SelectedVersion
    {
        get => _selectedVersion;
        set => this.RaiseAndSetIfChanged(ref _selectedVersion, value);
    }

    public ICommand RestoreCommand { get; }
    public event EventHandler<Holon>? RestoreRequested;

    public VersionHistoryViewModel(HolonViewModel item, HyperDriveService hyperDrive, DataService data)
    {
        HolonName = item.Name;
        HolonId = item.Id;
        _hyperDrive = hyperDrive;
        _data = data;

        var hasSelection = this.WhenAnyValue(x => x.SelectedVersion).Select(x => x is not null);
        RestoreCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedVersion?.Holon is not null)
                RestoreRequested?.Invoke(this, SelectedVersion.Holon);
        }, hasSelection);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        Versions.Clear();
        try
        {
            var history = await _hyperDrive.GetVersionHistoryAsync(HolonId);
            foreach (var h in history)
                Versions.Add(new HolonVersionViewModel(h));
            StatusText = $"{Versions.Count} version{(Versions.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class HolonVersionViewModel
{
    public Holon Holon { get; }
    public string VersionLabel => Holon.Version > 0 ? $"v{Holon.Version}" : "Original";
    public string Modified => Holon.ModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown";
    public string ModifiedBy => string.IsNullOrEmpty(Holon.ModifiedByAvatarId) ? "—" : Holon.ModifiedByAvatarId[..Math.Min(8, Holon.ModifiedByAvatarId.Length)];

    public HolonVersionViewModel(Holon holon) => Holon = holon;
}

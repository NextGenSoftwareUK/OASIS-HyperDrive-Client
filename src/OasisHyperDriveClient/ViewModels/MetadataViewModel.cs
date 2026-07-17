using OasisHyperDriveClient.Core.Models;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class MetadataViewModel : ViewModelBase
{
    private HolonViewModel? _holon;

    public HolonViewModel? Holon
    {
        get => _holon;
        set => this.RaiseAndSetIfChanged(ref _holon, value);
    }

    public IReadOnlyList<ProviderKeyRow> ProviderKeys => Holon?.ProviderKeys
        .Select(kv => new ProviderKeyRow(kv.Key, kv.Value))
        .ToList() ?? [];

    public IReadOnlyList<ReplicationRow> ReplicationStatus => Holon?.ReplicatedProviders
        .Select(p => new ReplicationRow(p, true))
        .ToList() ?? [];

    public void Load(HolonViewModel holon)
    {
        Holon = holon;
        this.RaisePropertyChanged(nameof(ProviderKeys));
        this.RaisePropertyChanged(nameof(ReplicationStatus));
    }
}

public record ProviderKeyRow(string Provider, string Key);
public record ReplicationRow(string Provider, bool IsReplicated);

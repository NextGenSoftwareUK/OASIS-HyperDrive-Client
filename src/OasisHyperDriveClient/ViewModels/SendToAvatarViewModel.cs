using System.Collections.ObjectModel;
using System.Reactive;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class SendToAvatarViewModel : ReactiveObject
{
    private readonly AvatarService _avatarService;
    private string _searchQuery = string.Empty;
    private AvatarInfo? _selectedAvatar;
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }
    public AvatarInfo? SelectedAvatar
    {
        get => _selectedAvatar;
        set => this.RaiseAndSetIfChanged(ref _selectedAvatar, value);
    }
    public bool   IsBusy        { get => _isBusy;         set => this.RaiseAndSetIfChanged(ref _isBusy, value); }
    public string StatusMessage { get => _statusMessage;  set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

    public ObservableCollection<AvatarInfo> SearchResults { get; } = new();

    public string ItemName { get; }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> SendCommand   { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event EventHandler<AvatarInfo>? SendRequested;
    public event EventHandler?             Cancelled;

    public SendToAvatarViewModel(string itemName, AvatarService avatarService)
    {
        ItemName = itemName;
        _avatarService = avatarService;

        var canSearch = this.WhenAnyValue(x => x.SearchQuery, x => x.IsBusy,
            (q, busy) => !string.IsNullOrWhiteSpace(q) && !busy);

        var canSend = this.WhenAnyValue(x => x.SelectedAvatar, x => x.IsBusy,
            (a, busy) => a is not null && !busy);

        SearchCommand = ReactiveCommand.CreateFromTask(DoSearchAsync, canSearch);
        SendCommand   = ReactiveCommand.Create(DoSend, canSend);
        CancelCommand = ReactiveCommand.Create(() => Cancelled?.Invoke(this, EventArgs.Empty));
    }

    private async Task DoSearchAsync()
    {
        IsBusy = true;
        SearchResults.Clear();
        try
        {
            var avatars = await _avatarService.SearchAvatarsAsync(SearchQuery);
            foreach (var a in avatars)
                SearchResults.Add(a);
            if (SearchResults.Count == 0)
                StatusMessage = "No avatars found.";
        }
        finally { IsBusy = false; }
    }

    private void DoSend()
    {
        if (SelectedAvatar is not null)
            SendRequested?.Invoke(this, SelectedAvatar);
    }
}

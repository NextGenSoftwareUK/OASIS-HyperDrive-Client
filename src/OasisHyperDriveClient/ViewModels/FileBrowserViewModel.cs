using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Auth;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;
using ReactiveUI;

namespace OasisHyperDriveClient.ViewModels;

public class FileBrowserViewModel : ViewModelBase
{
    private readonly DataService _data;
    private readonly HyperDriveService _hyperDrive;
    private readonly AuthService _auth;
    private readonly HyperDriveMonitorService? _monitor;

    private string _selectedContentType = "All";
    private string _selectedProvider = "All Providers";
    private string _searchQuery = string.Empty;
    private bool _isLoading;
    private string _statusText = "Ready";
    private HolonViewModel? _selectedItem;
    private bool _isGridView;

    // Breadcrumb navigation
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentPath = "/";

    public ObservableCollection<HolonViewModel> Items { get; } = [];
    public ObservableCollection<HolonViewModel> SelectedItems { get; } = [];
    public ObservableCollection<string> AvailableProviders { get; } = ["All Providers"];
    public ObservableCollection<string> ActiveProviderNames { get; } = [];
    public ObservableCollection<BreadcrumbSegment> Breadcrumbs { get; } = [new("/", "/")];

    public string SelectedContentType
    {
        get => _selectedContentType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedContentType, value);
            _ = LoadItemsAsync();
        }
    }

    public string SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProvider, value);
            _ = LoadItemsAsync();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }

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

    public HolonViewModel? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public bool IsGridView
    {
        get => _isGridView;
        set => this.RaiseAndSetIfChanged(ref _isGridView, value);
    }

    public string CurrentPath
    {
        get => _currentPath;
        private set => this.RaiseAndSetIfChanged(ref _currentPath, value);
    }

    public ICommand RefreshCommand            { get; }
    public ICommand DeleteCommand             { get; }
    public ICommand ViewMetadataCommand       { get; }
    public ICommand RenameCommand             { get; }
    public ICommand SendToAvatarCommand       { get; }
    public ICommand UploadCommand             { get; }
    public ICommand DownloadCommand           { get; }
    public ICommand SelectContentTypeCommand  { get; }
    public ICommand ToggleViewCommand         { get; }
    public ICommand BackCommand               { get; }
    public ICommand ForwardCommand            { get; }
    public ICommand NavigateUpCommand         { get; }
    public ICommand NavigateToCommand         { get; }
    public ICommand ViewOnProviderCommand     { get; }
    public ICommand BatchDeleteCommand        { get; }
    public ICommand BatchDownloadCommand      { get; }
    public ICommand ShowVersionHistoryCommand { get; }

    public static IReadOnlyList<string> ContentTypes { get; } =
        ["All", "File", "Holon", "NFT", "GeoNFT", "Avatar", "Keys"];

    public event EventHandler<HolonViewModel>? ViewMetadataRequested;
    public event EventHandler<HolonViewModel>? SendToAvatarRequested;
    public event EventHandler<HolonViewModel>? RenameRequested;
    public event EventHandler<HolonViewModel>? DeleteRequested;
    public event EventHandler<HolonViewModel>? DownloadRequested;
    public event EventHandler?                 UploadRequested;
    public event EventHandler<HolonViewModel>? VersionHistoryRequested;

    public FileBrowserViewModel(
        DataService data,
        HyperDriveService hyperDrive,
        AuthService auth,
        HyperDriveMonitorService? monitor = null)
    {
        _data = data;
        _hyperDrive = hyperDrive;
        _auth = auth;
        _monitor = monitor;

        if (_monitor is not null)
            _monitor.StateChanged += OnMonitorStateChanged;

        var hasSelection = this.WhenAnyValue(x => x.SelectedItem)
            .Select(x => x is not null);

        RefreshCommand = ReactiveCommand.CreateFromTask(LoadItemsAsync);

        DeleteCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) DeleteRequested?.Invoke(this, SelectedItem);
        }, hasSelection);

        ViewMetadataCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) ViewMetadataRequested?.Invoke(this, SelectedItem);
        }, hasSelection);

        RenameCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) RenameRequested?.Invoke(this, SelectedItem);
        }, hasSelection);

        SendToAvatarCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) SendToAvatarRequested?.Invoke(this, SelectedItem);
        }, hasSelection);

        UploadCommand = ReactiveCommand.Create(() => UploadRequested?.Invoke(this, EventArgs.Empty));

        DownloadCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) DownloadRequested?.Invoke(this, SelectedItem);
        }, hasSelection);

        SelectContentTypeCommand = ReactiveCommand.Create<string>(type =>
        {
            SelectedContentType = type;
        });

        ToggleViewCommand = ReactiveCommand.Create(() => IsGridView = !IsGridView);

        var canGoBack    = this.WhenAnyValue(x => x.CurrentPath).Select(_ => _backStack.Count > 0);
        var canGoForward = this.WhenAnyValue(x => x.CurrentPath).Select(_ => _forwardStack.Count > 0);
        var canGoUp      = this.WhenAnyValue(x => x.CurrentPath).Select(p => p != "/");

        NavigateUpCommand = ReactiveCommand.Create(() =>
        {
            var parent = CurrentPath.TrimEnd('/');
            var idx = parent.LastIndexOf('/');
            var up = idx <= 0 ? "/" : parent[..idx];
            _backStack.Push(CurrentPath);
            _forwardStack.Clear();
            CurrentPath = up;
            RebuildBreadcrumbs();
            _ = LoadItemsAsync();
        }, canGoUp);

        BackCommand = ReactiveCommand.Create(() =>
        {
            if (_backStack.Count == 0) return;
            _forwardStack.Push(CurrentPath);
            CurrentPath = _backStack.Pop();
            RebuildBreadcrumbs();
            _ = LoadItemsAsync();
        }, canGoBack);

        ForwardCommand = ReactiveCommand.Create(() =>
        {
            if (_forwardStack.Count == 0) return;
            _backStack.Push(CurrentPath);
            CurrentPath = _forwardStack.Pop();
            RebuildBreadcrumbs();
            _ = LoadItemsAsync();
        }, canGoForward);

        NavigateToCommand = ReactiveCommand.Create<string>(path =>
        {
            if (path == CurrentPath) return;
            _backStack.Push(CurrentPath);
            _forwardStack.Clear();
            CurrentPath = path;
            RebuildBreadcrumbs();
            _ = LoadItemsAsync();
        });

        ViewOnProviderCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is null) return;
            var url = BuildProviderUrl(SelectedItem);
            if (!string.IsNullOrEmpty(url))
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
                catch { /* ignore */ }
        }, hasSelection);

        // Track whether SelectedItems is non-empty via CollectionChanged
        var hasMultiSelectSubject = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        SelectedItems.CollectionChanged += (_, _) => hasMultiSelectSubject.OnNext(SelectedItems.Count > 0);

        BatchDeleteCommand = ReactiveCommand.Create(() =>
        {
            foreach (var item in SelectedItems.ToList())
                DeleteRequested?.Invoke(this, item);
        }, hasMultiSelectSubject);

        BatchDownloadCommand = ReactiveCommand.Create(() =>
        {
            foreach (var item in SelectedItems.ToList())
                DownloadRequested?.Invoke(this, item);
        }, hasMultiSelectSubject);

        ShowVersionHistoryCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem is not null) VersionHistoryRequested?.Invoke(this, SelectedItem);
        }, hasSelection);
    }

    public async Task InitialiseAsync()
    {
        await LoadProvidersAsync();
        await LoadItemsAsync();
    }

    private async Task LoadProvidersAsync()
    {
        var config = await _hyperDrive.GetConfigurationAsync();
        if (config?.EnabledProviders is null) return;

        AvailableProviders.Clear();
        AvailableProviders.Add("All Providers");
        foreach (var p in config.EnabledProviders)
            AvailableProviders.Add(p);
    }

    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        StatusText = "Loading...";

        try
        {
            var provider = SelectedProvider == "All Providers" ? null : SelectedProvider;
            var holonType = SelectedContentType == "All" ? "All" : SelectedContentType;

            var holons = await _data.LoadAllHolonsAsync(holonType, provider);

            Items.Clear();
            foreach (var h in holons)
            {
                var vm = HolonViewModel.FromHolon(h);
                if (string.IsNullOrEmpty(SearchQuery) ||
                    vm.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    Items.Add(vm);
                }
            }

            StatusText = $"{Items.Count} item{(Items.Count == 1 ? "" : "s")}";
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

    private async Task DeleteSelectedAsync()
    {
        if (SelectedItem is null) return;
        await DeleteAsync(SelectedItem, softDelete: true);
    }

    public async Task DeleteAsync(HolonViewModel item, bool softDelete)
    {
        var success = await _data.DeleteHolonAsync(item.Id, softDelete);
        if (success)
        {
            Items.Remove(item);
            StatusText = $"Deleted '{item.Name}'";
            if (SelectedItem == item) SelectedItem = null;
        }
        else
        {
            StatusText = $"Failed to delete '{item.Name}'";
        }
    }

    public async Task RenameAsync(HolonViewModel item, string newName)
    {
        var holon = new Core.Models.Holon
        {
            Id = item.Id,
            Name = newName,
            HolonType = item.HolonType
        };
        var saved = await _data.SaveHolonAsync(holon, null);
        if (saved is not null)
        {
            var idx = Items.IndexOf(item);
            if (idx >= 0)
            {
                var updated = Core.Models.HolonViewModel.FromHolon(saved);
                Items[idx] = updated;
                SelectedItem = updated;
            }
            StatusText = $"Renamed to '{newName}'";
        }
        else
        {
            StatusText = "Rename failed";
        }
    }

    public async Task SendToAvatarAsync(HolonViewModel item, Core.Models.AvatarInfo avatar)
    {
        StatusText = $"Sending '{item.Name}' to {avatar.Username}...";
        var holon = new Core.Models.Holon
        {
            Id = item.Id,
            Name = item.Name,
            HolonType = item.HolonType
        };
        var saved = await _data.SaveHolonAsync(holon, null);
        StatusText = saved is not null
            ? $"Sent '{item.Name}' to {avatar.Username}"
            : $"Send failed";
    }

    public async Task UploadFilesAsync(IReadOnlyList<IStorageFile> files)
    {
        if (_auth.CurrentAvatar is null) return;
        StatusText = $"Uploading {files.Count} file(s)...";
        IsLoading = true;
        var uploaded = 0;
        foreach (var file in files)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var ms = new System.IO.MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var ext = System.IO.Path.GetExtension(file.Name).TrimStart('.');
                var fileId = await _data.SaveFileAsync(
                    bytes, file.Name, ext, "application/octet-stream",
                    _auth.CurrentAvatar.Id, SelectedProvider == "All Providers" ? null : SelectedProvider);
                if (fileId.HasValue) uploaded++;
            }
            catch { /* skip individual failures */ }
        }
        await LoadItemsAsync();
        StatusText = $"Uploaded {uploaded}/{files.Count} file(s)";
    }

    public async Task DownloadAsync(HolonViewModel item, IStorageFile dest)
    {
        StatusText = $"Downloading '{item.Name}'...";
        IsLoading = true;
        try
        {
            var data = await _data.LoadFileAsync(item.Id,
                SelectedProvider == "All Providers" ? null : SelectedProvider);
            if (data is not null)
            {
                await using var stream = await dest.OpenWriteAsync();
                await stream.WriteAsync(data);
                StatusText = $"Downloaded '{item.Name}'";
            }
            else
            {
                StatusText = "Download failed";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void RequestSendToAvatar()
    {
        if (SelectedItem is not null)
            SendToAvatarRequested?.Invoke(this, SelectedItem);
    }

    private void RebuildBreadcrumbs()
    {
        Breadcrumbs.Clear();
        var parts = CurrentPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var accumulated = "/";
        Breadcrumbs.Add(new BreadcrumbSegment("Root", "/"));
        foreach (var part in parts)
        {
            accumulated = accumulated.TrimEnd('/') + "/" + part;
            Breadcrumbs.Add(new BreadcrumbSegment(part, accumulated));
        }
    }

    private static string BuildProviderUrl(HolonViewModel item)
    {
        return item.PrimaryProvider?.ToLowerInvariant() switch
        {
            "ethereum" or "eth" => $"https://etherscan.io/search?q={item.Id}",
            "ipfs"              => $"https://ipfs.io/ipfs/{item.Id}",
            "holochain"         => string.Empty,
            _                   => string.Empty
        };
    }

    private void OnMonitorStateChanged(object? sender, TrayStateInfo info)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            ActiveProviderNames.Clear();
            // AvailableProviders[0] is "All Providers" placeholder — skip it
            foreach (var p in AvailableProviders.Skip(1))
                ActiveProviderNames.Add(p);
        });
    }
}

public record BreadcrumbSegment(string Label, string Path);

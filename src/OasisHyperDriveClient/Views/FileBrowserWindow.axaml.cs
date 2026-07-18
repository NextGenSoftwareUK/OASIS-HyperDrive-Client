using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;
using OasisHyperDriveClient.Core.Services;
using OasisHyperDriveClient.Services;
using OasisHyperDriveClient.ViewModels;

namespace OasisHyperDriveClient.Views;

public partial class FileBrowserWindow : ReactiveWindow<FileBrowserViewModel>
{
    private readonly DataService? _data;
    private readonly HyperDriveService? _hyperDrive;
    private readonly AvatarService? _avatarService;
    private readonly AvaloniaNotificationService? _notifications;

    public FileBrowserWindow()
    {
        InitializeComponent();
    }

    public FileBrowserWindow(
        FileBrowserViewModel vm,
        DataService? data = null,
        AvatarService? avatarService = null,
        INotificationService? notifications = null,
        HyperDriveService? hyperDrive = null) : this()
    {
        DataContext = vm;
        _data = data;
        _hyperDrive = hyperDrive;
        _avatarService = avatarService;
        _notifications = notifications as AvaloniaNotificationService;

        vm.ViewMetadataRequested   += OnViewMetadata;
        vm.SendToAvatarRequested   += OnSendToAvatar;
        vm.RenameRequested         += OnRename;
        vm.DeleteRequested         += OnDeleteRequested;
        vm.UploadRequested         += OnUpload;
        vm.DownloadRequested       += OnDownload;
        vm.VersionHistoryRequested += OnVersionHistory;

        // Enable drag-and-drop onto the window
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_notifications is not null)
        {
            var manager = new WindowNotificationManager(TopLevel.GetTopLevel(this)!)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3
            };
            _notifications.Attach(manager);
        }

        if (DataContext is FileBrowserViewModel vm)
            await vm.InitialiseAsync();
    }

    private void OnViewMetadata(object? sender, HolonViewModel item)
    {
        var dialog = new MetadataWindow(item);
        dialog.ShowDialog(this);
    }

    private void OnSendToAvatar(object? sender, HolonViewModel item)
    {
        if (_avatarService is null) return;
        var vm = new SendToAvatarViewModel(item.Name, _avatarService);
        var dialog = new SendToAvatarDialog(vm);
        vm.SendRequested += async (_, avatar) =>
        {
            dialog.Close();
            await (DataContext as FileBrowserViewModel)!
                .SendToAvatarAsync(item, avatar);
        };
        vm.Cancelled += (_, _) => dialog.Close();
        dialog.ShowDialog(this);
    }

    private void OnRename(object? sender, HolonViewModel item)
    {
        var vm = new RenameViewModel(item.Name);
        var dialog = new RenameDialog(vm);
        vm.Confirmed += async (_, newName) =>
        {
            dialog.Close();
            await (DataContext as FileBrowserViewModel)!.RenameAsync(item, newName);
        };
        vm.Cancelled += (_, _) => dialog.Close();
        dialog.ShowDialog(this);
    }

    private void OnDeleteRequested(object? sender, HolonViewModel item)
    {
        var vm = new DeleteConfirmViewModel(item.Name);
        var dialog = new DeleteConfirmDialog(vm);
        vm.Confirmed += async (_, softDelete) =>
        {
            dialog.Close();
            await (DataContext as FileBrowserViewModel)!.DeleteAsync(item, softDelete);
        };
        vm.Cancelled += (_, _) => dialog.Close();
        dialog.ShowDialog(this);
    }

    private async void OnUpload(object? sender, EventArgs e)
    {
        if (_data is null) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = "Select files to upload"
        });

        if (files.Count == 0) return;

        var browserVm = (DataContext as FileBrowserViewModel)!;
        await browserVm.UploadFilesAsync(files);
    }

    private async void OnDownload(object? sender, HolonViewModel item)
    {
        if (_data is null) return;

        var dest = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = item.Name,
            Title = "Save file"
        });

        if (dest is null) return;

        var browserVm = (DataContext as FileBrowserViewModel)!;
        await browserVm.DownloadAsync(item, dest);
    }

    private void OnVersionHistory(object? sender, HolonViewModel item)
    {
        if (_hyperDrive is null || _data is null) return;
        var vm = new VersionHistoryViewModel(item, _hyperDrive, _data);
        var win = new VersionHistoryWindow(vm);
        vm.RestoreRequested += async (_, holon) =>
        {
            win.Close();
            await (DataContext as FileBrowserViewModel)!.RenameAsync(item, holon.Name);
        };
        win.ShowDialog(this);
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        var files = e.Data.GetFiles();
        if (files is null) return;

        var storageFiles = files.OfType<IStorageFile>().ToList();
        if (storageFiles.Count == 0) return;

        var browserVm = DataContext as FileBrowserViewModel;
        if (browserVm is null) return;

        await browserVm.UploadFilesAsync(storageFiles);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Hide instead of close so tray icon can re-open it
        e.Cancel = true;
        Hide();
    }
}

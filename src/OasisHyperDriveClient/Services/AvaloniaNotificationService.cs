using Avalonia.Controls.Notifications;
using OasisHyperDriveClient.Core.Services;

namespace OasisHyperDriveClient.Services;

public class AvaloniaNotificationService : INotificationService
{
    private WindowNotificationManager? _manager;

    public void Attach(WindowNotificationManager manager)
    {
        _manager = manager;
    }

    public void Show(string title, string message, NotificationLevel level = NotificationLevel.Info)
    {
        var type = level switch
        {
            NotificationLevel.Warning => NotificationType.Warning,
            NotificationLevel.Error   => NotificationType.Error,
            _                         => NotificationType.Information
        };

        var notification = new Notification(title, message, type,
            expiration: TimeSpan.FromSeconds(6));

        if (_manager is not null)
        {
            // Must be on UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                _manager.Show(notification));
        }
    }
}

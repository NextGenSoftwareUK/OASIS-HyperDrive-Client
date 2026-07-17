namespace OasisHyperDriveClient.Core.Services;

public enum NotificationLevel { Info, Warning, Error }

public interface INotificationService
{
    void Show(string title, string message, NotificationLevel level = NotificationLevel.Info);
}

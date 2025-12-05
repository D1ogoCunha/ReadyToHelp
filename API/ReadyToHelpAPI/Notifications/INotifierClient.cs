namespace readytohelpapi.Notifications;

/// <summary>
/// Client interface for sending notifications.
/// </summary>
public interface INotifierClient
{
    /// <summary>
    /// Sends a notification.
    /// </summary>
    Task NotifyAsync(NotificationRequest req, CancellationToken ct = default);

    /// <summary>
    /// Sends a notification for a specified duration.
    /// </summary>
    Task NotifyForNMinutesAsync(NotificationRequest req, int minutes = 5, CancellationToken ct = default);

    /// <summary>
    /// Sends a notification repeatedly.
    /// </summary>
    Task NotifyRepeatedAsync(NotificationRequest req, int total = 5, TimeSpan? interval = null, CancellationToken ct = default);
}
namespace readytohelpapi.Notifications;

using readytohelpapi.ResponsibleEntity.Models;

/// <summary>
/// Request model for notifications.
/// </summary>
public class NotificationRequest
{
    /// <summary>
    /// Type of the responsible entity.
    /// </summary>
    public ResponsibleEntityType Type { get; set; }

    /// <summary>
    /// Name of the responsible entity.
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// ID of the responsible entity.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// ID of the occurrence.
    /// </summary>
    public int OccurrenceId { get; set; }

    /// <summary>
    /// Title of the notification.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Latitude of the notification location.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of the notification location.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Message of the notification.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Timestamp of the notification.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
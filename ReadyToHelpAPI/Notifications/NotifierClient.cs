namespace readytohelpapi.Notifications;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>
/// Client for sending notifications.
/// </summary>
public class NotifierClient : INotifierClient
{
    private readonly HttpClient _http;
    private readonly ILogger<NotifierClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Constructor for NotifierClient.
    /// </summary>
    public NotifierClient(HttpClient http, ILogger<NotifierClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification.
    /// </summary>
    public async Task NotifyAsync(NotificationRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        try
        {
            using var resp = await _http.PostAsJsonAsync("/notify", req, JsonOpts, ct);
            resp.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Notification cancelled for OccurrenceId={OccId}", req.OccurrenceId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification failed for OccurrenceId={OccId}", req.OccurrenceId);
            throw;
        }
    }

    /// <summary>
    /// Sends a notification for a specified duration.
    /// </summary>
    public Task NotifyForNMinutesAsync(NotificationRequest req, int minutes = 5, CancellationToken ct = default)
        => NotifyRepeatedAsync(req, total: Math.Min(Math.Max(minutes, 1), 5), interval: TimeSpan.FromMinutes(1), ct: ct);

    /// <summary>
    /// Sends a notification repeatedly.
    /// </summary>
    public Task NotifyRepeatedAsync(NotificationRequest req, int total = 5, TimeSpan? interval = null, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var sends = Math.Clamp(total, 1, 5);
        var delay = interval is { } it && it > TimeSpan.Zero ? it : TimeSpan.FromMinutes(1);

        _ = Task.Run(async () =>
        {
            try
            {
                await NotifyAsync(CloneWithTimestamp(req), ct);
                if (sends == 1) return;

                using var timer = new PeriodicTimer(delay);
                var sent = 1;
                while (sent < sends && await timer.WaitForNextTickAsync(ct))
                {
                    await NotifyAsync(CloneWithTimestamp(req), ct);
                    sent++;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Repeated notifications cancelled for OccurrenceId={OccId}", req.OccurrenceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repeated notifications failed for OccurrenceId={OccId}", req.OccurrenceId);
            }
        }, ct);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clones a NotificationRequest with the current timestamp.
    /// </summary>
    /// <param name="src">The source NotificationRequest to clone.</param>
    /// <returns>A cloned NotificationRequest with the current timestamp.</returns>
    private static NotificationRequest CloneWithTimestamp(NotificationRequest src) => new NotificationRequest
    {
        Type = src.Type,
        EntityName = src.EntityName,
        EntityId = src.EntityId,
        OccurrenceId = src.OccurrenceId,
        Title = src.Title,
        Latitude = src.Latitude,
        Longitude = src.Longitude,
        Message = src.Message,
        Timestamp = DateTimeOffset.UtcNow
    };
}
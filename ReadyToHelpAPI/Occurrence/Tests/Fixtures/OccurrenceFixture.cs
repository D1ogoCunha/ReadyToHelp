using readytohelpapi.Occurrence.Models;
using GeoPointModel = readytohelpapi.GeoPoint.Models.GeoPoint;

namespace readytohelpapi.Occurrence.Tests.Fixtures;

/// <summary>
///   Provides helper methods to create or update Occurrence instances for testing.
/// </summary>
public static class OccurrenceFixture
{
    /// <summary>
    ///     Creates or updates an Occurrence object with the specified values.
    /// </summary>
    public static Models.Occurrence CreateOrUpdateOccurrence(
        Models.Occurrence? o = null,
        int? id = null,
        string title = "Default title",
        string description = "Default description",
        OccurrenceType type = OccurrenceType.FOREST_FIRE,
        OccurrenceStatus status = OccurrenceStatus.ACTIVE,
        PriorityLevel priority = PriorityLevel.MEDIUM,
        double proximityRadius = 100,
        DateTime? endDateTime = null,
        int reportCount = 0,
        int reportId = 0,
        int responsibleEntityId = 0,
        double latitude = 0,
        double longitude = 0
    )
    {
        o ??= new Models.Occurrence();
        if (id.HasValue) o.Id = id.Value;
        o.Title = title;
        o.Description = description;
        o.Type = type;
        o.Status = status;
        o.Priority = priority;
        o.ProximityRadius = proximityRadius;
        o.EndDateTime = endDateTime ?? DateTime.UtcNow.AddHours(1);
        o.ReportCount = reportCount;
        o.ReportId = reportId;
        o.ResponsibleEntityId = responsibleEntityId;
        o.Location = new GeoPointModel
        {
            Latitude = latitude,
            Longitude = longitude
        };

        return o;
    }
}
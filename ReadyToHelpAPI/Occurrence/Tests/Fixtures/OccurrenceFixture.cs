namespace readytohelpapi.Occurrence.Tests.Fixtures;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;

/// <summary>
///   Provides helper methods to create or update Occurrence instances for testing.
/// </summary>
public static class OccurrenceFixture
{
    /// <summary>
    ///     Creates or updates an Occurrence object with the specified values.
    /// </summary>
    public static Occurrence CreateOrUpdateOccurrence(
        Occurrence? o = null,
        OccurrenceFixtureDto? options = null
    )
    {
        options ??= new OccurrenceFixtureDto();
        o ??= new Occurrence();
        if (options.Id.HasValue)
            o.Id = options.Id.Value;
        o.Title = options.Title;
        o.Description = options.Description;
        o.Type = options.Type;
        o.Status = options.Status;
        o.Priority = options.Priority;
        o.ProximityRadius = options.ProximityRadius;
        o.EndDateTime = options.EndDateTime ?? DateTime.UtcNow.AddHours(1);
        o.ReportCount = options.ReportCount;
        o.ReportId = options.ReportId;
        o.ResponsibleEntityId = options.ResponsibleEntityId;
        o.Location = new GeoPoint { Latitude = options.Latitude, Longitude = options.Longitude };
        return o;
    }
}

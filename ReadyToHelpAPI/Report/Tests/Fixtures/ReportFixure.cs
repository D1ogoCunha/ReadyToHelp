namespace readytohelpapi.Report.Tests.Fixtures;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;

/// <summary>
/// Fixture class for creating Report instances for testing.
/// </summary>
public static class ReportFixture
{
    public static Report CreateOrUpdate(
        Report? r = null,
        int? id = null,
        string title = "Default title",
        string description = "Default description",
        int userId = 1,
        OccurrenceType type = OccurrenceType.ROAD_DAMAGE,
        GeoPoint? location = null
    )
    {
        location ??= new GeoPoint { Latitude = 41.3678, Longitude = -8.2012 };

        if (r == null)
        {
            r = new Report
            {
                Title = title,
                Description = description,
                UserId = userId,
                Type = type,
                Location = location,
            };

            if (id.HasValue)
                r.Id = id.Value;

            return r;
        }

        if (id.HasValue)
            r.Id = id.Value;
        r.Title = title;
        r.Description = description;
        r.UserId = userId;
        r.Type = type;
        r.Location ??= location;

        return r;
    }
}

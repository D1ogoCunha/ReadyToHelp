using readytohelpapi.Occurrence.Models;
using GeoPointModel = readytohelpapi.GeoPoint.Models.GeoPoint;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

/// <summary>
///    This class contains all unit tests related to the occurrence model.
/// </summary>
[Trait("Category", "Unit")]
public class TestOccurrenceModel
{
    /// <summary>
    ///    Test to verify that the default constructor initializes properties correctly.
    /// </summary>
    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var o = new Models.Occurrence();

        Assert.Equal(0, o.Id);
        Assert.Null(o.Title);
        Assert.Null(o.Description);
        Assert.Equal(OccurrenceStatus.ACTIVE, o.Status);
        Assert.Equal(0, o.ReportCount);
        Assert.Null(o.Location);
        Assert.InRange(o.CreationDateTime, before, DateTime.UtcNow.AddSeconds(2));
    }

    /// <summary>
    ///   Test to verify that the parameterized constructor sets properties correctly, including Location.
    /// </summary>
    [Fact]
    public void ParameterizedConstructor_SetsProperties_IncludingLocation()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var end = DateTime.UtcNow.AddHours(1);
        var loc = new GeoPointModel(9.25, -13.5);

        var o = new Models.Occurrence(
            id: 5,
            title: "T",
            description: "D",
            type: OccurrenceType.FLOOD,
            status: OccurrenceStatus.IN_PROGRESS,
            priority: PriorityLevel.HIGH,
            proximityRadius: 250d,
            endDateTime: end,
            reportCount: 3,
            reportId: 10,
            responsibleEntityId: 77,
            location: loc
        );

        Assert.Equal(5, o.Id);
        Assert.Equal("T", o.Title);
        Assert.Equal("D", o.Description);
        Assert.Equal(OccurrenceType.FLOOD, o.Type);
        Assert.Equal(OccurrenceStatus.IN_PROGRESS, o.Status);
        Assert.Equal(PriorityLevel.HIGH, o.Priority);
        Assert.Equal(250d, o.ProximityRadius);
        Assert.Equal(end, o.EndDateTime);
        Assert.Equal(3, o.ReportCount);
        Assert.Equal(10, o.ReportId);
        Assert.Equal(77, o.ResponsibleEntityId);

        Assert.Same(loc, o.Location);
        Assert.Equal(9.25, o.Location.Latitude);
        Assert.Equal(-13.5, o.Location.Longitude);

        Assert.InRange(o.CreationDateTime, before, DateTime.UtcNow.AddSeconds(2));
    }
}
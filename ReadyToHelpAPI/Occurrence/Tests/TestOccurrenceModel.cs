namespace readytohelpapi.Occurrence.Tests;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;
using readytohelpapi.Occurrence.Models;
using Xunit;

/// <summary>
///    This class contains all unit tests related to the occurrence model.
/// </summary>
[Trait("Category", "Unit")]
public class TestOccurrenceModel
{
    /// <summary>
    ///   Test to verify that the parameterized constructor sets properties correctly, including Location.
    /// </summary>
    [Fact]
    public void ParameterizedConstructor_SetsProperties_IncludingLocation()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var end = DateTime.UtcNow.AddHours(1);
        var loc = new GeoPoint(9.25, -13.5);

        var dto = new OccurrenceCreateDto
        {
            Id = 5,
            Title = "T",
            Description = "D",
            Type = OccurrenceType.FLOOD,
            Status = OccurrenceStatus.IN_PROGRESS,
            Priority = PriorityLevel.HIGH,
            ProximityRadius = 250d,
            EndDateTime = end,
            ReportCount = 3,
            ReportId = 10,
            ResponsibleEntityId = 77,
            Location = loc,
        };

        var o = new Occurrence(dto);

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

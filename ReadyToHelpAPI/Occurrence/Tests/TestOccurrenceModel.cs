using readytohelpapi.Occurrence.Models;
using Xunit;

namespace readytohelpapi.Occurrence.Tests;

public class TestOccurrenceModel
{
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
        Assert.InRange(o.CreationDateTime, before, DateTime.UtcNow.AddSeconds(2));
    }

    [Fact]
    public void ParameterizedConstructor_SetsProperties()
    {
        var end = DateTime.UtcNow.AddHours(1);
        var o = new Models.Occurrence(
            id: 5,
            title: "T",
            description: "D",
            type: OccurrenceType.FLOOD,
            status: OccurrenceStatus.IN_PROGRESS,
            priority: PriorityLevel.HIGH,
            proximityRadius: 250,
            endDateTime: end,
            reportCount: 3,
            reportId: 10,
            responsibleEntityId: 77
        );

        Assert.Equal(5, o.Id);
        Assert.Equal("T", o.Title);
        Assert.Equal("D", o.Description);
        Assert.Equal(OccurrenceType.FLOOD, o.Type);
        Assert.Equal(OccurrenceStatus.IN_PROGRESS, o.Status);
        Assert.Equal(PriorityLevel.HIGH, o.Priority);
        Assert.Equal(250, o.ProximityRadius);
        Assert.Equal(end, o.EndDateTime);
        Assert.Equal(3, o.ReportCount);
        Assert.Equal(10, o.ReportId);
        Assert.Equal(77, o.ResponsibleEntityId);
    }
}
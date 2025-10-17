using readytohelpapi.Occurrence.Models;

namespace readytohelpapi.Occurrence.Tests.Fixtures;

public static class OccurrenceFixture
{
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
        int responsibleEntityId = 0)
    {
        o ??= new Models.Occurrence();
        if (id.HasValue) o.Id = id.Value;
        o.Title = title;
        o.Description = description;
        o.Type = type;
        o.Status = status;
        o.Priority = priority;
        o.ProximityRadius = proximityRadius;
        o.EndDateTime = endDateTime ?? default;
        o.ReportCount = reportCount;
        o.ReportId = reportId;
        o.ResponsibleEntityId = responsibleEntityId;
        return o;
    }
}
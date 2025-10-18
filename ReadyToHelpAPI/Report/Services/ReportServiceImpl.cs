namespace readytohelpapi.Report.Services;

using readytohelpapi.GeoPoint.Miscellaneous;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Report.Models;



public class ReportServiceImpl : IReportService
{
    private readonly IReportRepository reportRepository;
    private readonly IOccurrenceService occurrenceService;

    private const double DefaultProximityRadiusMeters = 50d;
    private const int triggerActivate = 3;

    public ReportServiceImpl(IReportRepository reportRepository, IOccurrenceService occurrenceService)
    {
        this.reportRepository = reportRepository;
        this.occurrenceService = occurrenceService;
    }

    public (Report report, Occurrence occurrence) Create(Report report)
    {
        if (report is null)
            throw new ArgumentNullException(nameof(report));
        if (string.IsNullOrWhiteSpace(report.Title))
            throw new ArgumentException("Title is required.", nameof(report.Title));
        if (string.IsNullOrWhiteSpace(report.Description))
            throw new ArgumentException("Description is required.", nameof(report.Description));
        if (report.UserId <= 0)
            throw new ArgumentException("UserId must be greater than zero.", nameof(report.UserId));
        if (report.Location is null)
            throw new ArgumentException("Location is required.", nameof(report.Location));

        var duplicatedOccurence = FindNearbyOccurrenceOfSameType(report, DefaultProximityRadiusMeters);

        var createdReport = reportRepository.Create(report);

        if (duplicatedOccurence != null)
        {
            duplicatedOccurence.ReportCount += 1;
            if (duplicatedOccurence.ReportCount >= triggerActivate &&
                duplicatedOccurence.Status == OccurrenceStatus.WAITING)
            {
                duplicatedOccurence.Status = OccurrenceStatus.ACTIVE;
            }

            occurrenceService.Update(duplicatedOccurence);

            return (createdReport, duplicatedOccurence);
        }

        var occurrence = new Occurrence
        {
            Title = createdReport.Title,
            Description = createdReport.Description,
            Type = report.Type,
            Priority = report.Priority,
            ProximityRadius = DefaultProximityRadiusMeters,
            Status = OccurrenceStatus.WAITING,
            EndDateTime = default,
            ReportCount = 1,
            ReportId = createdReport.Id,
            ResponsibleEntityId = 0,
            Location = report.Location
        };

        var createdOccurrence = occurrenceService.Create(occurrence);

        return (createdReport, createdOccurrence);
    }

    private Occurrence? FindNearbyOccurrenceOfSameType(Report newReport, double radiusMeters)
    {
        var sameTypeOccurrences = occurrenceService.GetOccurrencesByType(newReport.Type);
        if (sameTypeOccurrences == null || sameTypeOccurrences.Count == 0) return null;

        foreach (var occ in sameTypeOccurrences)
        {
            var anchorReport = reportRepository.GetById(occ.ReportId);
            if (anchorReport?.Location == null) continue;

            var d = GeoUtils.DistanceMeters(
                newReport.Location.Latitude,
                newReport.Location.Longitude,
                anchorReport.Location.Latitude,
                anchorReport.Location.Longitude
            );

            if (d <= radiusMeters)
                return occ;
        }

        return null;
    }
}
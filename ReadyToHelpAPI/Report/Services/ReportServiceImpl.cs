namespace readytohelpapi.Report.Services;

using readytohelpapi.GeoPoint.Miscellaneous;
using readytohelpapi.Notifications;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Report.Models;
using readytohelpapi.ResponsibleEntity.Services;

/// <summary>
///  Implementation of the report service.
/// </summary>
public class ReportServiceImpl : IReportService
{
    private readonly IReportRepository reportRepository;
    private readonly IOccurrenceService occurrenceService;
    private readonly IResponsibleEntityService responsibleEntityService;
    private readonly INotifierClient notifierClient;
    private const double DefaultProximityRadiusMeters = 50d;
    private const int triggerActivate = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportServiceImpl"/> class.
    /// </summary>
    /// <param name="reportRepository">The report repository.</param>
    /// <param name="occurrenceService">The occurrence service.</param>
    /// <param name="responsibleEntityService">The responsible entity service.</param>
    /// <param name="notifierClient">The notifier client.</param>
    public ReportServiceImpl(IReportRepository reportRepository, IOccurrenceService occurrenceService, IResponsibleEntityService responsibleEntityService, INotifierClient notifierClient)
    {
        this.reportRepository = reportRepository;
        this.occurrenceService = occurrenceService;
        this.responsibleEntityService = responsibleEntityService;
        this.notifierClient = notifierClient;
    }

    /// <summary>
    /// Creates a new report
    /// Triggers occurrence creation or update as needed.
    /// Notiofies responsible entity when occurrence is activated.
    /// </summary>
    /// <param name="report">The report to create.</param>
    /// <returns>The created report the associated occurrence, that has the responsible entity.</returns>
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

        var responsibleEntity = responsibleEntityService.FindResponsibleEntity(
            report.Type,
            report.Location.Latitude,
            report.Location.Longitude
        );

        var duplicatedOccurence = FindNearbyOccurrenceOfSameType(report, DefaultProximityRadiusMeters);

        var createdReport = reportRepository.Create(report);

        if (duplicatedOccurence != null)
        {
            duplicatedOccurence.ReportCount += 1;
            if (duplicatedOccurence.ReportCount >= triggerActivate &&
                duplicatedOccurence.Status == OccurrenceStatus.WAITING)
            {
                duplicatedOccurence.Status = OccurrenceStatus.ACTIVE;

                var reqDup = new NotificationRequest
                {
                    Type = responsibleEntity?.Type ?? report.Type.GetResponsibleEntityType(),
                    EntityId = responsibleEntity?.Id,
                    EntityName = responsibleEntity?.Name,
                    OccurrenceId = duplicatedOccurence.Id,
                    Title = report.Title,
                    Latitude = report.Location.Latitude,
                    Longitude = report.Location.Longitude,
                    Message = $"Novo relatório para ocorrência existente ({duplicatedOccurence.Id})."
                };
                _ = notifierClient.NotifyForNMinutesAsync(reqDup, minutes: 5);
            }

            occurrenceService.Update(duplicatedOccurence);

            return (createdReport, duplicatedOccurence);
        }

        var occurrence = new Occurrence
        {
            Title = createdReport.Title,
            Description = createdReport.Description,
            Type = report.Type,
            ProximityRadius = DefaultProximityRadiusMeters,
            Status = OccurrenceStatus.WAITING,
            EndDateTime = default,
            ReportCount = 1,
            ReportId = createdReport.Id,
            ResponsibleEntityId = responsibleEntity?.Id ?? 0,
            Location = report.Location
        };

        var createdOccurrence = occurrenceService.Create(occurrence);

        return (createdReport, createdOccurrence);
    }

    private Occurrence? FindNearbyOccurrenceOfSameType(Report newReport, double radiusMeters)
    {
        if (newReport?.Location is null) return null;

        var sameTypeOccurrences = occurrenceService.GetOccurrencesByType(newReport.Type);
        if (sameTypeOccurrences == null || sameTypeOccurrences.Count == 0) return null;

        foreach (var occ in sameTypeOccurrences)
        {
            var anchorReport = occ.ReportId.HasValue ? reportRepository.GetById(occ.ReportId.Value) : null;
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
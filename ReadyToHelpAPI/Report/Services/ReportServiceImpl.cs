namespace readytohelpapi.Report.Services;

using readytohelpapi.GeoPoint.Miscellaneous;
using readytohelpapi.Notifications;
using readytohelpapi.Occurrence.DTOs;
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
    public ReportServiceImpl(
        IReportRepository reportRepository,
        IOccurrenceService occurrenceService,
        IResponsibleEntityService responsibleEntityService,
        INotifierClient notifierClient
    )
    {
        this.reportRepository = reportRepository;
        this.occurrenceService = occurrenceService;
        this.responsibleEntityService = responsibleEntityService;
        this.notifierClient = notifierClient;
    }

    /// <inheritdoc />
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

        var duplicatedOccurrence = FindNearbyOccurrenceOfSameType(report);
        var createdReport = reportRepository.Create(report);

        if (duplicatedOccurrence != null)
        {
            return HandleDuplicateOccurrence(
                duplicatedOccurrence,
                createdReport,
                report,
                responsibleEntity
            );
        }

        var createdOccurrence = CreateNewOccurrence(createdReport, report, responsibleEntity);
        return (createdReport, createdOccurrence);
    }

    /// <summary>
    /// Handles the logic for a duplicate occurrence.
    /// </summary>
    /// <param name="duplicatedOccurrence">The duplicated occurrence.</param>
    /// <param name="createdReport">The created report.</param>
    /// <param name="report">The original report.</param>
    /// <param name="responsibleEntity">The responsible entity.</param>
    /// <returns>The updated report and the duplicated occurrence.</returns>
    private (Report, Occurrence) HandleDuplicateOccurrence(
        Occurrence duplicatedOccurrence,
        Report createdReport,
        Report report,
        ResponsibleEntity.Models.ResponsibleEntity? responsibleEntity
    )
    {
        duplicatedOccurrence.ReportCount += 1;
        if (
            duplicatedOccurrence.ReportCount >= triggerActivate
            && duplicatedOccurrence.Status == OccurrenceStatus.WAITING
        )
        {
            duplicatedOccurrence.Status = OccurrenceStatus.ACTIVE;
            NotifyResponsibleEntity(
                duplicatedOccurrence,
                report,
                responsibleEntity,
                isDuplicate: true
            );
        }

        occurrenceService.Update(duplicatedOccurrence);
        return (createdReport, duplicatedOccurrence);
    }

    /// <summary>
    /// Creates a new occurrence in case no duplicate is found.
    /// </summary>
    /// <param name="createdReport">The created report.</param>
    /// <param name="report">The original report.</param>
    /// <param name="responsibleEntity">The responsible entity.</param>
    /// <returns>The created occurrence.</returns>
    private Occurrence CreateNewOccurrence(
        Report createdReport,
        Report report,
        ResponsibleEntity.Models.ResponsibleEntity? responsibleEntity
    )
    {
        var occurrence = new Occurrence(
            new OccurrenceCreateDto
            {
                Title = createdReport.Title,
                Description = createdReport.Description,
                Type = report.Type,
                Status = OccurrenceStatus.WAITING,
                EndDateTime = default,
                ReportCount = 1,
                ReportId = createdReport.Id,
                ResponsibleEntityId = responsibleEntity?.Id ?? 0,
                Location = report.Location,
            }
        );

        return occurrenceService.Create(occurrence);
    }

    /// <summary>
    /// Notifies the responsible entity about the occurrence.
    /// </summary>
    /// <param name="occurrence">The occurrence to notify about.</param>
    /// <param name="report">The report associated with the occurrence.</param>
    /// <param name="responsibleEntity">The responsible entity to notify.</param>
    /// <param name="isDuplicate">Indicates if the occurrence is a duplicate.</param>
    private void NotifyResponsibleEntity(
        Occurrence occurrence,
        Report report,
        ResponsibleEntity.Models.ResponsibleEntity? responsibleEntity,
        bool isDuplicate
    )
    {
        var req = new NotificationRequest
        {
            Type = responsibleEntity?.Type ?? report.Type.GetResponsibleEntityType(),
            EntityId = responsibleEntity?.Id,
            EntityName = responsibleEntity?.Name,
            OccurrenceId = occurrence.Id,
            Title = report.Title,
            Latitude = report.Location.Latitude,
            Longitude = report.Location.Longitude,
            Message = isDuplicate
                ? $"Novo relatório para ocorrência existente ({occurrence.Id})."
                : "Ocorrência reportada.",
        };
        _ = notifierClient.NotifyForNMinutesAsync(req, minutes: 5);
    }

    /// <summary>
    /// Finds a nearby occurrence of the same type within a specified radius.
    /// </summary>
    /// <param name="newReport">The new report to find occurrences for.</param>
    /// <param name="radiusMeters">The radius within which to search for occurrences.</param>
    /// <returns>The found occurrence, or null if none is found.</returns>
    private Occurrence? FindNearbyOccurrenceOfSameType(Report newReport)
    {
        if (newReport?.Location is null)
            return null;

        var sameTypeOccurrences = occurrenceService.GetOccurrencesByType(newReport.Type);
        if (sameTypeOccurrences == null || sameTypeOccurrences.Count == 0)
            return null;

        foreach (var occ in sameTypeOccurrences)
        {
            var occRadius =
                occ.ProximityRadius > 0 ? occ.ProximityRadius : DefaultProximityRadiusMeters;

            var anchorReport = occ.ReportId.HasValue
                ? reportRepository.GetById(occ.ReportId.Value)
                : null;
            if (anchorReport?.Location == null)
                continue;

            var d = GeoUtils.DistanceMeters(
                newReport.Location.Latitude,
                newReport.Location.Longitude,
                anchorReport.Location.Latitude,
                anchorReport.Location.Longitude
            );

            if (d <= occRadius)
                return occ;
        }

        return null;
    }
}

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

        report.IsDuplicate = false;
        var createdReport = reportRepository.Create(report);

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
            ResponsibleEntityId = 0
        };

        var createdOccurrence = occurrenceService.Create(occurrence);

        return (createdReport, createdOccurrence);
    }
}
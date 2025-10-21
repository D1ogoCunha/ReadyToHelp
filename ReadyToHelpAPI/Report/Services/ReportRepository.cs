namespace readytohelpapi.Report.Services;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Report.Models;

/// <summary>
///    Repository for managing report data.
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly AppDbContext reportContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReportRepository" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ReportRepository(AppDbContext context)
    {
        reportContext = context;
    }

    /// <summary>
    ///     Creates a new report in the database.
    /// </summary>
    /// <param name="report">The report to create.</param>
    /// <returns>The created report.</returns>
    public Report Create(Report report)
    {
        if (report == null) throw new ArgumentNullException(nameof(report));
        try
        {
            if (report.ReportDateTime == default)
                report.ReportDateTime = DateTime.UtcNow;

            var created = reportContext.Reports.Add(report).Entity;
            reportContext.SaveChanges();
            return created;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to create report", ex);
        }
    }

    /// <summary>
    ///     Retrieves a report by its ID.
    /// </summary>
    /// <param name="id">The ID of the report.</param>
    /// <returns>The report if found; otherwise, null.</returns>
    public Report? GetById(int id)
    {
        if (id <= 0) return null;
        return reportContext.Reports
            .AsNoTracking()
            .FirstOrDefault(r => r.Id == id);
    }
}
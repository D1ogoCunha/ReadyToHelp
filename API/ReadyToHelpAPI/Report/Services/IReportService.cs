namespace readytohelpapi.Report.Services;

using readytohelpapi.Occurrence.Models;
using readytohelpapi.Report.Models;

/// <summary>
/// Defines the contract for report services.
/// </summary>
public interface IReportService
{
    /// <summary>
    ///  Creates a report and trigger the creation of an occurrence.
    /// </summary>
    /// <param name="report">The report to be created.</param>
    (Report report, Occurrence occurrence) Create(Report report);
}

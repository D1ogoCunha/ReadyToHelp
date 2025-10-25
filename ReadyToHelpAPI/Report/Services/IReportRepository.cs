namespace readytohelpapi.Report.Services;

using Models;

/// <summary>
///  Defines the contract for report repository to manage data.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    ///   Creates a report in the repository.
    /// </summary>
    /// <param name="report">The report object to be created.</param>
    /// <returns>The created report entity.</returns>
    Report Create(Report report);

    /// <summary>
    ///  Retrieves a report by ID.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>The report entity if found; otherwise, null.</returns>
    Report? GetById(int id);
}
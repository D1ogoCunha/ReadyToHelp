namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Models;
using System.Collections.Generic;

/// <summary>
///     Defines the contract for an occurrence repository to manage data.
/// </summary>
public interface IOccurrenceRepository
{
    /// <summary>
    ///     Creates an occurrence in the repository.
    /// </summary>
    /// <param name="occurrence">The occurrence object to be created.</param>
    /// <returns>The created occurrence entity.</returns>
    Occurrence Create(Occurrence occurrence);

    /// <summary>
    ///     Retrieves an occurrence by ID.
    /// </summary>
    /// <param name="id">The occurrence ID.</param>
    /// <returns>The occurrence entity if found; otherwise, null.</returns>
    Occurrence? GetOccurrenceById(int id);

    /// <summary>
    ///     Updates an occurrence in the repository.
    /// </summary>
    /// <param name="occurrence">The occurrence to update.</param>
    /// <returns>The updated occurrence entity.</returns>
    //Occurrence Update(Occurrence occurrence);

    /// <summary>
    ///     Deletes an occurrence by ID.
    /// </summary>
    /// <param name="id">The occurrence ID.</param>
    /// <returns>The deleted occurrence entity if found; otherwise, null.</returns>
    //Occurrence? Delete(int id);

    /// <summary>
    ///     Retrieves occurrences by partial or full title.
    /// </summary>
    /// <param name="title">The title to search for.</param>
    /// <returns>A list of occurrences that match the title.</returns>
    List<Occurrence> GetOccurrenceByTitle(string title);

    /// <summary>
    ///     Retrieves a paginated, filtered, and sorted list of occurrences.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field by which to sort the results.</param>
    /// <param name="sortOrder">The sort order, either "asc" or "desc".</param>
    /// <param name="filter">The string to filter the occurrence data.</param>
    /// <returns>A paginated, sorted, and filtered list of occurrences.</returns>
    List<Occurrence> GetAllOccurrences(int pageNumber, int pageSize, string sortBy, string sortOrder, string filter);

    /// <summary>
    ///     Retrieves all occurrences by type.
    /// </summary>
    /// <param name="type">The type of the occurrence.</param>
    /// <returns>A list of occurrences of the specified type.</returns>
    List<Occurrence> GetOccurrencesByType(OccurrenceType type);

    /// <summary>
    ///     Retrieves all occurrences by the specified status.
    /// </summary>
    /// <param name="status">The status of the occurrences to retrieve.</param>
    /// <returns>A list of occurrences with the specified status.</returns>
    List<Occurrence> GetOccurrencesByStatus(OccurrenceStatus status);

    /// <summary>
    ///     Retrieves all occurrences by the specified priority level.
    /// </summary>
    /// <param name="priority">The priority level of the occurrence.</param>
    /// <returns>A list of occurrences of the specified priority level.</returns>
    List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority);

    /// <summary>
    ///     Retrieves all occurrences with status ACTIVE.
    /// </summary>
    /// <returns>A list of active occurrences.</returns>
    List<Occurrence> GetAllActiveOccurrences();

    /// <summary>
    ///     Retrieves an occurrence by reportId.
    /// </summary>
    /// <param name="reportId">The report identifier.</param>
    /// <returns>The occurrence with the specified reportId or null.</returns>
    Occurrence? GetByReportId(int reportId);
}
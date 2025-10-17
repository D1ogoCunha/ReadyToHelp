namespace readytohelpapi.Occurrence.Services;

using readytohelpapi.Occurrence.Models;
using System.Collections.Generic;

/// <summary>
///  Defines the contract for occurrence-related operations.
/// </summary>
public interface IOccurrenceService
{
    /// <summary>
    ///     Creates an occurrence.
    /// </summary>
    /// <param name="occurrence">The occurrence object to be created.</param>
    /// <returns>The created occurrence entity.</returns>
    Occurrence Create(Occurrence occurrence);

    /// <summary>
    ///     Updates an occurrence.
    /// </summary>
    /// <param name="occurrence">The occurrence object to be updated.</param>
    /// <returns>The updated occurrence entity.</returns>
    Occurrence Update(Occurrence occurrence);

    /// <summary>
    ///     Deletes an occurrence.
    /// </summary>
    /// <param name="id">The occurrence id to be deleted.</param>
    /// <returns>The deleted occurrence entity, if successfully found.</returns>
    Occurrence Delete(int id);

    /// <summary>
    ///     Retrieves an occurrence by its unique identifier.
    /// </summary>
    /// <param name="id">The occurrence ID.</param>
    /// <returns>The occurrence entity if found.</returns>
    Occurrence GetOccurrenceById(int id);

    /// <summary>
    ///     Retrieves a list of occurrences by partial title.
    /// </summary>
    /// <param name="title">The partial or full title to search for.</param>
    /// <returns>A list of occurrences that match the search criteria.</returns>
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
    ///     Retrieves all occurrences for a specific responsible entity.
    /// </summary>
    /// <param name="responsibleEntityId">The ID of the responsible entity.</param>
    /// <returns>A list of occurrences for the responsible entity.</returns>
    //List<Occurrence> GetOccurrencesByResponsibleEntity(int responsibleEntityId);

    /// <summary>
    ///     Retrieves all occurrences by type.
    /// </summary>
    /// <param name="type">The type of the occurrence.</param>
    /// <returns>A list of occurrences of the specified type.</returns>
    List<Occurrence> GetOccurrencesByType(OccurrenceType type);

    /// <summary>
    ///     Retrieves all occurrences with status ACTIVE.
    /// </summary>
    /// <returns>A list of occurrences with status ACTIVE.</returns>
    List<Occurrence> GetAllActiveOccurrences();

    /// <summary>
    ///     Retrieves all occurrences by the specified priority level.
    /// </summary>
    /// <param name="priority">The priority level of the occurrence.</param>
    /// <returns>A list of occurrences of the specified priority level.</returns>
    List<Occurrence> GetOccurrencesByPriority(PriorityLevel priority);
    
}
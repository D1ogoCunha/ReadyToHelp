namespace readytohelpapi.Feedback.Services;

using System.Collections.Generic;
using readytohelpapi.Feedback.Models;

/// <summary>
///   Defines the contract for feedback repository operations.
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>
    ///   Creates a new feedback.
    /// </summary>
    /// <param name="feedback">The feedback to create.</param>
    /// <returns>The created feedback.</returns>
    Feedback Create(Feedback feedback);

    /// <summary>
    ///   Gets a feedback by its ID.
    /// </summary>
    /// <param name="id">The ID of the feedback.</param>
    /// <returns>The feedback with the specified ID, or null if not found.</returns>
    Feedback? GetFeedbackById(int id);

    /// <summary>
    ///   Gets all feedbacks.
    /// </summary>
    /// <returns>All feedbacks.</returns>
    List<Feedback> GetAllFeedbacks();

    /// <summary>
    ///   Gets feedbacks by occurrence ID.
    /// </summary>
    /// <param name="occurrenceId">The ID of the occurrence.</param>
    /// <returns>The feedbacks for the specified occurrence.</returns>
    List<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId);

    /// <summary>
    ///   Gets feedbacks by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The feedback for the specified user.</returns>
    List<Feedback> GetFeedbacksByUserId(int userId);

    /// <summary>
    ///   Checks if a user exists by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the user exists, otherwise false.</returns>
    bool UserExists(int userId);

    /// <summary>
    ///   Checks if an occurrence exists by occurrence ID.
    /// </summary>
    /// <param name="occurrenceId">The ID of the occurrence.</param>
    /// <returns>True if the occurrence exists, otherwise false.</returns>
    bool OccurrenceExists(int occurrenceId);
}

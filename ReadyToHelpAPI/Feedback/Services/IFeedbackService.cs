namespace readytohelpapi.Feedback.Services
{
    using System.Collections.Generic;
    using readytohelpapi.Feedback.Models;

    /// <summary>
    ///   Interface for feedback service.
    /// </summary>
    public interface IFeedbackService
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
        /// <returns>A collection of all feedbacks.</returns>
        IEnumerable<Feedback> GetAllFeedbacks();

        /// <summary>
        ///   Gets feedbacks by occurrence ID.
        /// </summary>
        /// <param name="occurrenceId">The ID of the occurrence.</param>
        /// <returns>A collection of feedbacks for the specified occurrence.</returns>
        IEnumerable<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId);

        /// <summary>
        ///  Gets feedbacks by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A collection of feedback for the specified user.</returns>
        IEnumerable<Feedback> GetFeedbacksByUserId(int userId);
    }
}

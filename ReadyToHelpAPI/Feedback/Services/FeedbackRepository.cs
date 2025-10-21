namespace readytohelpapi.Feedback.Services;

using System.Collections.Generic;
using System.Linq;
using readytohelpapi.Common.Data;
using readytohelpapi.Feedback.Models;

/// <summary>
///   Implementation of the feedback repository.
/// </summary>
public class FeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext context;

    /// <summary>
    ///  Initializes a new instance of the <see cref="FeedbackRepository"/> class.
    /// </summary>
    /// <param name="context">The feedback database context.</param>
    public FeedbackRepository(AppDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    ///   Creates a new feedback in the database.
    /// </summary>
    /// <param name="feedback">The feedback to create.</param>
    /// <returns>The created feedback.</returns>
    public Feedback Create(Feedback feedback)
    {
        var created = context.Feedbacks.Add(feedback).Entity;
        context.SaveChanges();
        return created;
    }

    /// <summary>
    ///   Gets a feedback by its ID.
    /// </summary>
    /// <param name="id">The ID of the feedback.</param>
    /// <returns>The feedback with the specified ID, or null if not found.</returns>
    public Feedback? GetFeedbackById(int id) => context.Feedbacks.FirstOrDefault(f => f.Id == id);

    /// <summary>
    ///   Gets all feedbacks from the database.
    /// </summary>
    /// <returns>A collection of all feedbacks.</returns>
    public List<Feedback> GetAllFeedbacks() => context.Feedbacks.ToList();

    /// <summary>
    ///   Gets feedbacks by occurrence ID.
    /// </summary>
    /// <param name="occurrenceId">The ID of the occurrence.</param>
    /// <returns>A collection of feedbacks for the specified occurrence.</returns>
    public List<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId) =>
        context.Feedbacks.Where(f => f.OccurrenceId == occurrenceId).ToList();

    /// <summary>
    ///   Gets feedbacks by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of feedbacks for the specified user.</returns>
    public List<Feedback> GetFeedbacksByUserId(int userId) =>
        context.Feedbacks.Where(f => f.UserId == userId).ToList();
}

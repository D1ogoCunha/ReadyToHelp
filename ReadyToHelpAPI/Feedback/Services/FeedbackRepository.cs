namespace readytohelpapi.Feedback.Services;

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

    /// <inheritdoc />
    public Feedback Create(Feedback feedback)
    {
        if (feedback == null)
        {
            throw new ArgumentNullException(nameof(feedback));
        }
        try
        {
            var created = context.Feedbacks.Add(feedback).Entity;
            context.SaveChanges();

            return created;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to create feedback", ex);
        }
    }

    /// <inheritdoc />
    public Feedback? GetFeedbackById(int id) => context.Feedbacks.FirstOrDefault(f => f.Id == id);

    /// <inheritdoc />
    public List<Feedback> GetAllFeedbacks() => context.Feedbacks.ToList();

    /// <inheritdoc />
    public List<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId) =>
        context.Feedbacks.Where(f => f.OccurrenceId == occurrenceId).ToList();

    /// <inheritdoc />
    public List<Feedback> GetFeedbacksByUserId(int userId) =>
        context.Feedbacks.Where(f => f.UserId == userId).ToList();

    /// <inheritdoc />
    public bool UserExists(int userId) => context.Users.Any(u => u.Id == userId);

    /// <inheritdoc />
    public bool OccurrenceExists(int occurrenceId) => context.Occurrences.Any(o => o.Id == occurrenceId);
}

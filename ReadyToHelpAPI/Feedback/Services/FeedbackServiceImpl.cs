namespace readytohelpapi.Feedback.Services;

using System;
using System.Collections.Generic;
using readytohelpapi.Common.Data;
using readytohelpapi.Feedback.Models;

/// <summary>
///   Implementation of feedback service operations.
/// </summary>
public class FeedbackServiceImpl : IFeedbackService
{
    private readonly IFeedbackRepository repo;
    private readonly AppDbContext context;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FeedbackServiceImpl"/> class.
    /// </summary>
    /// <param name="repo">The feedback repository.</param>
    /// <param name="context">The application database context.</param>
    public FeedbackServiceImpl(IFeedbackRepository repo, AppDbContext context)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    ///   Creates a new feedback.
    /// </summary>
    /// <param name="feedback">The feedback to create.</param>
    /// <returns>The created feedback.</returns>
    public Feedback Create(Feedback feedback)
    {
        if (feedback == null)
            throw new ArgumentNullException(nameof(feedback));

        var user = this.context.Users.Find(feedback.UserId);
        if (user == null)
            throw new ArgumentException(
                $"User with id {feedback.UserId} does not exist",
                nameof(feedback.UserId)
            );

        var occurrence = this.context.Occurrences.Find(feedback.OccurrenceId);
        if (occurrence == null)
            throw new ArgumentException(
                $"Occurrence with id {feedback.OccurrenceId} does not exist",
                nameof(feedback.OccurrenceId)
            );

        feedback.FeedbackDateTime = DateTime.UtcNow;
        return repo.Create(feedback);
    }

    /// <summary>
    ///   Gets feedback by its Id.
    /// </summary>
    /// <param name="id">The feedback Id.</param>
    /// <returns>The feedback if found; otherwise, null.</returns>
    public Feedback? GetFeedbackById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(id));

        return repo.GetFeedbackById(id);
    }

    /// <summary>
    ///   Gets all feedbacks.
    /// </summary>
    /// <returns>All feedbacks.</returns>
    public List<Feedback> GetAllFeedbacks() => repo.GetAllFeedbacks();

    /// <summary>
    ///   Gets feedbacks by occurrence Id.
    /// </summary>
    /// <param name="occurrenceId">The occurrence Id.</param>
    /// <returns>The feedbacks associated with the specified occurrence Id.</returns>
    public List<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId)
    {
        if (occurrenceId <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(occurrenceId));

        var occurrence = this.context.Occurrences.Find(occurrenceId);
        if (occurrence == null)
            throw new ArgumentException(
                $"Occurrence with id {occurrenceId} does not exist",
                nameof(occurrenceId)
            );

        return repo.GetFeedbacksByOccurrenceId(occurrenceId);
    }

    /// <summary>
    ///   Gets feedbacks by user Id.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <returns>The feedbacks associated with the specified user Id.</returns>
    public List<Feedback> GetFeedbacksByUserId(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(userId));

        var user = this.context.Users.Find(userId);
        if (user == null)
            throw new ArgumentException($"User with id {userId} does not exist", nameof(userId));

        return repo.GetFeedbacksByUserId(userId);
    }
}

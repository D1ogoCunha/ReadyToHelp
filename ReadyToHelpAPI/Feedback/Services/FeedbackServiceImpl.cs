namespace readytohelpapi.Feedback.Services;

using System;
using System.Collections.Generic;
using readytohelpapi.Feedback.Models;

/// <summary>
///   Implementation of feedback service operations.
/// </summary>
public class FeedbackServiceImpl : IFeedbackService
{
    private readonly IFeedbackRepository repo;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FeedbackServiceImpl"/> class.
    /// </summary>
    /// <param name="repo">The feedback repository.</param>
    public FeedbackServiceImpl(IFeedbackRepository repo)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    /// <inheritdoc />
    public Feedback Create(Feedback feedback)
    {
        if (feedback == null)
            throw new ArgumentNullException(nameof(feedback));

        if (!repo.UserExists(feedback.UserId))
            throw new ArgumentException(
                $"User with id {feedback.UserId} does not exist",
                nameof(feedback.UserId)
            );

        if (!repo.OccurrenceExists(feedback.OccurrenceId))
            throw new ArgumentException(
                $"Occurrence with id {feedback.OccurrenceId} does not exist",
                nameof(feedback.OccurrenceId)
            );

        feedback.FeedbackDateTime = DateTime.UtcNow;
        return repo.Create(feedback);
    }

    /// <inheritdoc />
    public Feedback? GetFeedbackById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(id));

        return repo.GetFeedbackById(id);
    }

    /// <inheritdoc />
    public List<Feedback> GetAllFeedbacks() => repo.GetAllFeedbacks();

    /// <inheritdoc />
    public List<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId)
    {
        if (occurrenceId <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(occurrenceId));

        if (!repo.OccurrenceExists(occurrenceId))
            throw new ArgumentException(
                $"Occurrence with id {occurrenceId} does not exist",
                nameof(occurrenceId)
            );

        return repo.GetFeedbacksByOccurrenceId(occurrenceId);
    }

    /// <inheritdoc />
    public List<Feedback> GetFeedbacksByUserId(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Id must be a positive integer", nameof(userId));

        if (!repo.UserExists(userId))
            throw new ArgumentException($"User with id {userId} does not exist", nameof(userId));

        return repo.GetFeedbacksByUserId(userId);
    }
}
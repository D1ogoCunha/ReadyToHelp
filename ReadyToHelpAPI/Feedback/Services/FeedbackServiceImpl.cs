namespace readytohelpapi.Feedback.Services;

using System;
using System.Collections.Generic;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Models;

/// <summary>
///   Implementation of feedback service operations.
/// </summary>
public class FeedbackServiceImpl : IFeedbackService
{
    private readonly IFeedbackRepository repo;
    private readonly IOccurrenceService occurrenceService;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FeedbackServiceImpl"/> class.
    /// </summary>
    /// <param name="repo">The feedback repository.</param>
    /// <param name="occurrenceService">The occurrence service.</param>
    public FeedbackServiceImpl(IFeedbackRepository repo, IOccurrenceService? occurrenceService)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.occurrenceService = occurrenceService ?? throw new ArgumentNullException(nameof(occurrenceService));
    }

    /// <inheritdoc />
    public Feedback Create(Feedback feedback)
    {
        if (feedback == null)
            throw new ArgumentNullException(nameof(feedback));

        if (!repo.UserExists(feedback.UserId))
            throw new ArgumentException(
                $"User with id {feedback.UserId} does not exist",
                nameof(feedback)
            );

        if (!repo.OccurrenceExists(feedback.OccurrenceId))
            throw new ArgumentException(
                $"Occurrence with id {feedback.OccurrenceId} does not exist",
                nameof(feedback)
            );

        var occCheck = occurrenceService.GetOccurrenceById(feedback.OccurrenceId);
        if (occCheck != null && occCheck.Status == OccurrenceStatus.WAITING)
            throw new InvalidOperationException("Cannot submit feedback for an occurrence with WAITING status.");
        feedback.FeedbackDateTime = DateTime.UtcNow;

        var created = repo.Create(feedback);
        try
        {
            if (created != null && !created.IsConfirmed)
            {
                var allForOccurrence = repo.GetFeedbacksByOccurrenceId(created.OccurrenceId) ?? new List<Feedback>();
                var negativeCount = allForOccurrence.Count(f => f != null && !f.IsConfirmed);

                if (negativeCount >= 5)
                {
                    var occ = occurrenceService.GetOccurrenceById(created.OccurrenceId);
                    if (occ != null && occ.EndDateTime == default)
                    {
                        occ.Status = OccurrenceStatus.CLOSED;
                        occ.EndDateTime = DateTime.UtcNow;
                        occurrenceService.Update(occ);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process occurrence status update based on feedback.", ex);
        }

        return created;
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
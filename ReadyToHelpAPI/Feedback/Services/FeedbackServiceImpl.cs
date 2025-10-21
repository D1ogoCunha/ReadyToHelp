namespace readytohelpapi.Feedback.Services;

using System;
using System.Collections.Generic;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Common.Data;

public class FeedbackServiceImpl : IFeedbackService
{
    private readonly IFeedbackRepository repo;
    private readonly AppDbContext context;

    public FeedbackServiceImpl(
        IFeedbackRepository repo,
        AppDbContext context
    )
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

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

    public Feedback? GetFeedbackById(int id) => repo.GetFeedbackById(id);

    public IEnumerable<Feedback> GetAllFeedbacks() => repo.GetAllFeedbacks();

    public IEnumerable<Feedback> GetFeedbacksByOccurrenceId(int occurrenceId) =>
        repo.GetFeedbacksByOccurrenceId(occurrenceId);

    public IEnumerable<Feedback> GetFeedbacksByUserId(int userId) => repo.GetFeedbacksByUserId(userId);
}
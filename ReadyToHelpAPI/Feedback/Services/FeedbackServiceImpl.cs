namespace readytohelpapi.Feedback.Services
{
    using System;
    using System.Collections.Generic;
    using readytohelpapi.Feedback.Models;
    using readytohelpapi.Occurrence.Data;
    using readytohelpapi.User.Data;

    public class FeedbackServiceImpl : IFeedbackService
    {
        private readonly IFeedbackRepository repo;
        private readonly UserContext userContext;
        private readonly OccurrenceContext occurrenceContext;

        public FeedbackServiceImpl(
            IFeedbackRepository repo,
            UserContext userContext,
            OccurrenceContext occurrenceContext
        )
        {
            this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
            this.userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            this.occurrenceContext =
                occurrenceContext ?? throw new ArgumentNullException(nameof(occurrenceContext));
        }

        public Feedback Create(Feedback feedback)
        {
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));

            var user = this.userContext.Users.Find(feedback.UserId);
            if (user == null)
                throw new ArgumentException(
                    $"User with id {feedback.UserId} does not exist",
                    nameof(feedback.UserId)
                );

            var occurrence = this.occurrenceContext.Occurrences.Find(feedback.OccurrenceId);
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
}

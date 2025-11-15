namespace readytohelpapi.Feedback.Tests.Fixtures;

using System;
using readytohelpapi.Feedback.Models;

/// <summary>
///   Helper class to create Feedback objects with default values for tests.
/// </summary>
public static class FeedbackFixture
{
    /// <summary>
    ///   Creates a Feedback object with default values.
    /// </summary>
    public static Feedback Create(
        int id = 0,
        int userId = 1,
        int occurrenceId = 1,
        bool isConfirmed = false,
        DateTime? date = null)
    {
        return new Feedback
        {
            Id = id,
            UserId = userId,
            OccurrenceId = occurrenceId,
            IsConfirmed = isConfirmed,
            FeedbackDateTime = date ?? DateTime.UtcNow
        };
    }
}

namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for feedbacks.
/// </summary>
public class FeedbackStatsDto
{
    /// <summary>
    /// Gets or sets the total number of feedbacks.
    /// </summary>
    public int TotalFeedbacks { get; set; }

    /// <summary>
    /// Gets or sets the number of new feedbacks in the last 24 hours.
    /// </summary>
    public int NewFeedbacksLast24Hours { get; set; }

    /// <summary>
    /// Gets or sets the number of new feedbacks in the last 7 days.
    /// </summary>
    public int NewFeedbacksLast7Days { get; set; }

    /// <summary>
    /// Gets or sets the number of new feedbacks in the last 30 days.
    /// </summary>
    public int NewFeedbacksLast30Days { get; set; }

    /// <summary>
    /// Gets or sets the number of unique users who provided feedback.
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of confirmed feedbacks.
    /// </summary>
    public int ConfirmedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of not confirmed feedbacks.
    /// </summary>
    public int NotConfirmedCount { get; set; }

    /// <summary>
    /// Gets or sets the confirmation rate of feedbacks.
    /// </summary>
    public double ConfirmationRate { get; set; }

    /// <summary>
    /// Gets or sets the average number of feedbacks per day in the last 30 days.
    /// </summary>
    public double AverageFeedbacksPerDayLast30 { get; set; }

    /// <summary>
    /// Gets or sets for user with most feedbacks.
    /// </summary>
    public int TopFeedbackUserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user with most feedbacks.
    /// </summary>
    public string? TopFeedbackUserName { get; set; }

    /// <summary>
    /// Gets or sets the count of feedbacks from the top user.
    /// </summary>
    public int TopFeedbackUserCount { get; set; }

    /// <summary>
    /// Gets or sets the date of the first feedback.
    /// </summary>
    public DateTime? FirstFeedbackDate { get; set; }

    /// <summary>
    /// Gets or sets the date of the last feedback.
    /// </summary>
    public DateTime? LastFeedbackDate { get; set; }
}

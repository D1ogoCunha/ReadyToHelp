namespace readytohelpapi.Dashboard.DTOs;

public class FeedbackStatsDto
{
    public int TotalFeedbacks { get; set; }

    public int NewFeedbacksLast24Hours { get; set; }
    public int NewFeedbacksLast7Days { get; set; }
    public int NewFeedbacksLast30Days { get; set; }

    public int UniqueUsers { get; set; }

    public int ConfirmedCount { get; set; }
    public int NotConfirmedCount { get; set; }
    public double ConfirmationRate { get; set; } // 0..100

    public double AverageFeedbacksPerDayLast30 { get; set; }

    public int TopFeedbackUserId { get; set; }
    public string? TopFeedbackUserName { get; set; }
    public int TopFeedbackUserCount { get; set; }

    public DateTime? FirstFeedbackDate { get; set; }
    public DateTime? LastFeedbackDate { get; set; }
}
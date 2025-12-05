namespace readytohelpapi.Dashboard.DTOs;

/// <summary>
/// Statistics aggregated for users.
/// </summary>
public class UserStatsDto
{
    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of admins.
    /// </summary>
    public int Admins { get; set; }

    /// <summary>
    /// Gets or sets the number of managers.
    /// </summary>
    public int Managers { get; set; }

    /// <summary>
    /// Gets or sets the number of citizens.
    /// </summary>
    public int Citizens { get; set; }

    /// <summary>
    /// Gets or sets the number of users with reports.
    /// </summary>
    public int UsersWithReports { get; set; }

    /// <summary>
    /// Gets or sets the number of users with feedbacks.
    /// </summary>
    public int UsersWithFeedbacks { get; set; }

    /// <summary>
    /// Gets or sets the number of users with both reports and feedbacks.
    /// </summary>
    public int UsersWithBoth { get; set; }

    /// <summary>
    /// Gets or sets the number of users without reports or feedbacks.
    /// </summary>
    public int UsersWithoutReportsOrFeedbacks { get; set; }

    /// <summary>
    /// Gets or sets the most active user id.
    /// </summary>
    public int MostActiveUserId { get; set; }

    /// <summary>
    /// Gets or sets the most active user name.
    /// </summary>
    public string? MostActiveUserName { get; set; }

    /// <summary>
    /// Gets or sets the most active user reports count.
    /// </summary>
    public int MostActiveUserReports { get; set; }

    /// <summary>
    /// Gets or sets the most active user feedbacks count.
    /// </summary>
    public int MostActiveUserFeedbacks { get; set; }
}

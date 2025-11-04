namespace readytohelpapi.Dashboard.DTOs;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int Admins { get; set; }
    public int Managers { get; set; }
    public int Citizens { get; set; }
    public int NewUsersLast30Days { get; set; }
    public int UsersWithReports { get; set; }
    public int UsersWithFeedbacks { get; set; }
    public int UsersWithBoth { get; set; }
    public int UsersWithoutReportsOrFeedbacks { get; set; }
    public int MostActiveUserId { get; set; }
    public string? MostActiveUserName { get; set; }
    public int MostActiveUserReports { get; set; }
    public int MostActiveUserFeedbacks { get; set; }
}

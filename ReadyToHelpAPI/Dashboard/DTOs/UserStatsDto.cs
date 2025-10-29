namespace readytohelpapi.Dashboard.DTOs;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int Admins { get; set; }
    public int Managers { get; set; }
    public int Citizens { get; set; }
    public int NewUsersLast30Days { get; set; }
}
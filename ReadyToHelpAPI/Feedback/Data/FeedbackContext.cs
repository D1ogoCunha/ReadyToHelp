namespace readytohelpapi.Feedback.Data;

using Microsoft.EntityFrameworkCore;
using readytohelpapi.Feedback.Models;

public class FeedbackContext : DbContext
{
    public FeedbackContext(DbContextOptions<FeedbackContext> options)
        : base(options) { }

    public DbSet<Feedback> Feedbacks { get; set; }
}


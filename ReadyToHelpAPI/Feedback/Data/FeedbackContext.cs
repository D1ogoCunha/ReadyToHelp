namespace readytohelpapi.Feedback.Data
{
    using Microsoft.EntityFrameworkCore;
    using readytohelpapi.Feedback.Models;

    public class FeedbackContext : DbContext
    {
        public FeedbackContext(DbContextOptions<FeedbackContext> options)
            : base(options) { }

        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("feedback");

                entity.HasKey(f => f.Id).HasName("PK_feedback");

                entity.Property(f => f.FeedbackDateTime).IsRequired();
                entity.Property(f => f.IsConfirmed).IsRequired();

                entity
                    .HasOne(f => f.Occurrence)
                    .WithMany()
                    .HasForeignKey(f => f.OccurrenceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_feedback_occurrence");

                entity
                    .HasOne(f => f.User)
                    .WithMany()
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_feedback_user");
            });
        }
    }
}

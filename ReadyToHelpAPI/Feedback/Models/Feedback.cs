namespace readytohelpapi.Feedback.Models;

using System.ComponentModel.DataAnnotations;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.User.Models;

/// <summary>
/// Represents feedback provided by a user for a specific occurrence.
/// </summary>
public class Feedback
{
    /// <summary>
    /// Gets or sets the unique identifier for the feedback.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the occurrence associated with the feedback.
    /// </summary>
    [Required]
    public int OccurrenceId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who provided the feedback.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the feedback was provided.
    /// </summary>
    public DateTime FeedbackDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the user confirms the occurrence.
    /// </summary>
    public bool IsConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the occurrence associated with the feedback.
    /// </summary>
    public Occurrence? Occurrence { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the user who provided the feedback.
    /// </summary>
    public User? User { get; set; }
}


namespace readytohelpapi.Feedback.Tests;

using System;
using Xunit;
using readytohelpapi.Feedback.Models;


/// <summary>
/// This class contains all unit tests related to the feedback model.
/// </summary>
public class TestFeedbackModel
{
    /// <summary>
    /// Tests the default constructor of the Feedback model.
    /// </summary>
    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var f = new Feedback();

        Assert.Equal(0, f.Id);
        Assert.Equal(0, f.OccurrenceId);
        Assert.Equal(0, f.UserId);
        Assert.False(f.IsConfirmed);
        Assert.InRange(f.FeedbackDateTime, before, DateTime.UtcNow.AddSeconds(2));
    }

    /// <summary>
    /// Tests the initialization of the Feedback model with specific values.
    /// </summary>
    [Fact]
    public void Initialization_SetsPropertiesAndTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var fb = new Feedback
        {
            OccurrenceId = 42,
            UserId = 7,
            IsConfirmed = true
        };

        Assert.Equal(42, fb.OccurrenceId);
        Assert.Equal(7, fb.UserId);
        Assert.True(fb.IsConfirmed);
        Assert.InRange(fb.FeedbackDateTime, before, DateTime.UtcNow.AddSeconds(2));
    }
}
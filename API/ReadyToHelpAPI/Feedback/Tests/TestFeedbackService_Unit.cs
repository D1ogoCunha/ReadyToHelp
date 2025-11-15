namespace readytohelpapi.Feedback.Tests;

using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Models;

/// <summary>
///  This class contains all unit tests related to the feedback service.
/// </summary>
[Trait("Category", "Unit")]
public class TestFeedbackService_Unit
{
    private readonly Mock<IFeedbackRepository> mockRepo;
    private readonly IFeedbackService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFeedbackService_Unit"/> class.
    /// </summary>
    public TestFeedbackService_Unit()
    {
        mockRepo = new Mock<IFeedbackRepository>();
        var mockOccurrenceService = new Mock<IOccurrenceService>();
        service = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);
    }

    /// <summary>
    /// Tests the Create method with null feedback.
    /// </summary>
    [Fact]
    public void Create_NullFeedback_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => service.Create(null!));
    }

    /// <summary>
    /// Tests the Create method when the user does not exist.
    /// </summary>
    [Fact]
    public void Create_UserNotFound_ThrowsArgumentException()
    {
        var fb = new Feedback { UserId = 42, OccurrenceId = 1, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(It.IsAny<int>())).Returns(false);

        var ex = Assert.Throws<ArgumentException>(() => service.Create(fb));
        Assert.Contains("User", ex.Message, StringComparison.OrdinalIgnoreCase);
        mockRepo.Verify(r => r.Create(It.IsAny<Feedback>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method when the occurrence does not exist.
    /// </summary>
    [Fact]
    public void Create_OccurrenceNotFound_ThrowsArgumentException()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 99, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(It.IsAny<int>())).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(It.IsAny<int>())).Returns(false);

        var ex = Assert.Throws<ArgumentException>(() => service.Create(fb));
        Assert.Contains("Occurrence", ex.Message, StringComparison.OrdinalIgnoreCase);
        mockRepo.Verify(r => r.Create(It.IsAny<Feedback>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with valid feedback.
    /// </summary>
    [Fact]
    public void Create_ValidFeedback_SetsDateAndCallsRepo()
    {
        var input = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>()))
                .Returns<Feedback>(f => { f.Id = 10; return f; });

        var result = service.Create(input);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(1, result.UserId);
        Assert.Equal(2, result.OccurrenceId);
        Assert.True(result.FeedbackDateTime != default);
        Assert.True((DateTime.UtcNow - result.FeedbackDateTime).TotalSeconds < 10);
        mockRepo.Verify(r => r.Create(It.IsAny<Feedback>()), Times.Once);
    }

    /// <summary>
    /// Tests the GetFeedbackById method with invalid IDs.
    /// </summary>
    [Fact]
    public void GetFeedbackById_InvalidId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => service.GetFeedbackById(0));
        Assert.Throws<ArgumentException>(() => service.GetFeedbackById(-1));
    }

    /// <summary>
    /// Tests the GetFeedbackById method with valid ID.
    /// </summary>
    [Fact]
    public void GetFeedbackById_ReturnsRepoValue()
    {
        var sample = new Feedback { Id = 1, UserId = 1, OccurrenceId = 2, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow };
        mockRepo.Setup(r => r.GetFeedbackById(1)).Returns(sample);

        var res = service.GetFeedbackById(1);

        Assert.Equal(sample, res);
    }

    /// <summary>
    /// Tests the GetAllFeedbacks method.
    /// </summary>
    [Fact]
    public void GetAllFeedbacks_ReturnsRepoList()
    {
        var list = new List<Feedback>
            {
                new Feedback { Id = 1, UserId = 1, OccurrenceId = 2, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow },
                new Feedback { Id = 2, UserId = 2, OccurrenceId = 3, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow }
            };
        mockRepo.Setup(r => r.GetAllFeedbacks()).Returns(list);

        var res = service.GetAllFeedbacks();

        Assert.Equal(list, res);
    }

    /// <summary>
    /// Tests the GetFeedbacksByOccurrenceId method with invalid ID.
    /// </summary>
    [Fact]
    public void GetFeedbacksByOccurrenceId_Invalid_Throws()
    {
        Assert.Throws<ArgumentException>(() => service.GetFeedbacksByOccurrenceId(0));
    }

    /// <summary>
    /// Tests the GetFeedbacksByOccurrenceId method when occurrence does not exist.
    /// </summary>
    [Fact]
    public void GetFeedbacksByOccurrenceId_OccurrenceNotFound_Throws()
    {
        mockRepo.Setup(r => r.OccurrenceExists(It.IsAny<int>())).Returns(false);
        Assert.Throws<ArgumentException>(() => service.GetFeedbacksByOccurrenceId(5));
    }

    /// <summary>
    /// Tests the GetFeedbacksByOccurrenceId method with valid ID.
    /// </summary>
    [Fact]
    public void GetFeedbacksByOccurrenceId_ReturnsRepoList()
    {
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        var expected = new List<Feedback> { new Feedback { Id = 7, OccurrenceId = 2, UserId = 1, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow } };
        mockRepo.Setup(r => r.GetFeedbacksByOccurrenceId(2)).Returns(expected);

        var res = service.GetFeedbacksByOccurrenceId(2);

        Assert.Equal(expected, res);
        mockRepo.Verify(r => r.GetFeedbacksByOccurrenceId(2), Times.Once);
    }

    /// <summary>
    /// Tests the GetFeedbacksByUserId method with invalid ID.
    /// </summary>
    [Fact]
    public void GetFeedbacksByUserId_Invalid_Throws()
    {
        Assert.Throws<ArgumentException>(() => service.GetFeedbacksByUserId(0));
    }

    /// <summary>
    /// Tests the GetFeedbacksByUserId method when user does not exist.
    /// </summary>
    [Fact]
    public void GetFeedbacksByUserId_UserNotFound_Throws()
    {
        mockRepo.Setup(r => r.UserExists(It.IsAny<int>())).Returns(false);
        Assert.Throws<ArgumentException>(() => service.GetFeedbacksByUserId(5));
    }

    /// <summary>
    /// Tests the GetFeedbacksByUserId method with valid ID.
    /// </summary>
    [Fact]
    public void GetFeedbacksByUserId_ReturnsRepoList()
    {
        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        var expected = new List<Feedback> { new Feedback { Id = 9, UserId = 1, OccurrenceId = 2, IsConfirmed = true, FeedbackDateTime = DateTime.UtcNow } };
        mockRepo.Setup(r => r.GetFeedbacksByUserId(1)).Returns(expected);

        var res = service.GetFeedbacksByUserId(1);

        Assert.Equal(expected, res);
        mockRepo.Verify(r => r.GetFeedbacksByUserId(1), Times.Once);
    }

    /// <summary>
    /// Tests the Create method when the occurrence is in WAITING status.
    /// </summary>
    [Fact]
    public void Create_OccurrenceWaiting_ThrowsInvalidOperationException()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(2))
            .Returns(new Occurrence { Status = OccurrenceStatus.WAITING });

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.Create(fb));
        Assert.Contains("WAITING", ex.Message, StringComparison.OrdinalIgnoreCase);
        mockRepo.Verify(r => r.Create(It.IsAny<Feedback>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with negative feedback below threshold.
    /// </summary>
    [Fact]
    public void Create_NegativeFeedback_LessThanThreshold_DoesNotCloseOccurrence()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>()))
                .Returns<Feedback>(f => { f.Id = 11; return f; });
        mockRepo.Setup(r => r.GetFeedbacksByOccurrenceId(2))
                .Returns(new List<Feedback>
                {
                        new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow },
                        new Feedback { UserId = 2, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow },
                        new Feedback { UserId = 3, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow },
                });

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService
            .SetupSequence(s => s.GetOccurrenceById(2))
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE })
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE, EndDateTime = default });

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var result = svc.Create(fb);

        Assert.NotNull(result);
        mockOccurrenceService.Verify(s => s.Update(It.IsAny<Occurrence>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method with negative feedback reaching threshold.
    /// </summary>
    [Fact]
    public void Create_NegativeFeedback_ReachesThreshold_ClosesOccurrenceAndCallsUpdate()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>()))
                .Returns<Feedback>(f => { f.Id = 12; return f; });
        mockRepo.Setup(r => r.GetFeedbacksByOccurrenceId(2))
                .Returns(Enumerable.Range(0, 5).Select(_ =>
                    new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow }).ToList());

        var occForCheck = new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE };
        var occForUpdate = new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE, EndDateTime = default };

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService
            .SetupSequence(s => s.GetOccurrenceById(2))
            .Returns(occForCheck)
            .Returns(occForUpdate);

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var result = svc.Create(fb);

        Assert.NotNull(result);
        mockOccurrenceService.Verify(s => s.Update(It.Is<Occurrence>(
            o => o.Status == OccurrenceStatus.CLOSED && o.EndDateTime != default)), Times.Once);
    }

    /// <summary>
    /// Tests the Create method with negative feedback reaching threshold but occurrence already ended.
    /// </summary>
    [Fact]
    public void Create_NegativeFeedback_ReachesThreshold_OccurrenceAlreadyEnded_NoUpdate()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>()))
                .Returns<Feedback>(f => { f.Id = 13; return f; });
        mockRepo.Setup(r => r.GetFeedbacksByOccurrenceId(2))
                .Returns(Enumerable.Range(0, 5).Select(_ =>
                    new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow }).ToList());

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService
            .SetupSequence(s => s.GetOccurrenceById(2))
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE })
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE, EndDateTime = DateTime.UtcNow });

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var result = svc.Create(fb);

        Assert.NotNull(result);
        mockOccurrenceService.Verify(s => s.Update(It.IsAny<Occurrence>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method when occurrence update throws an exception.
    /// </summary>
    [Fact]
    public void Create_UpdateThrows_RethrowsWrappedInvalidOperationException()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>()))
                .Returns<Feedback>(f => { f.Id = 14; return f; });
        mockRepo.Setup(r => r.GetFeedbacksByOccurrenceId(2))
                .Returns(Enumerable.Range(0, 5).Select(_ =>
                    new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = false, FeedbackDateTime = DateTime.UtcNow }).ToList());

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService
            .SetupSequence(s => s.GetOccurrenceById(2))
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE })
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE, EndDateTime = default });

        mockOccurrenceService
            .Setup(s => s.Update(It.IsAny<Occurrence>()))
            .Throws(new InvalidOperationException("db fail"));

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.Create(fb));
        Assert.Contains("Failed to process occurrence status update", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    /// <summary>
    /// Tests the Create method when the same user already submitted feedback within the last hour.
    /// </summary>
    [Fact]
    public void Create_DuplicateWithinHour_ThrowsArgumentException()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.HasRecentFeedback(1, 2, It.IsAny<DateTime>())).Returns(true);

        var ex = Assert.Throws<ArgumentException>(() => service.Create(fb));
        Assert.Contains("within the last hour", ex.Message, StringComparison.OrdinalIgnoreCase);
        mockRepo.Verify(r => r.Create(It.IsAny<Feedback>()), Times.Never);
    }

    /// <summary>
    /// Tests the Create method when the repository returns null.
    /// </summary>
    [Fact]
    public void Create_RepoReturnsNull_ThrowsInvalidOperationException()
    {
        var fb = new Feedback { UserId = 1, OccurrenceId = 2, IsConfirmed = true };

        mockRepo.Setup(r => r.UserExists(1)).Returns(true);
        mockRepo.Setup(r => r.OccurrenceExists(2)).Returns(true);
        mockRepo.Setup(r => r.HasRecentFeedback(1, 2, It.IsAny<DateTime>())).Returns(false);
        mockRepo.Setup(r => r.Create(It.IsAny<Feedback>())).Returns((Feedback)null!);

        var mockOccurrenceService = new Mock<IOccurrenceService>();
        mockOccurrenceService.Setup(s => s.GetOccurrenceById(2))
            .Returns(new Occurrence { Id = 2, Status = OccurrenceStatus.ACTIVE });

        var svc = new FeedbackServiceImpl(mockRepo.Object, mockOccurrenceService.Object);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.Create(fb));
        Assert.Contains("Failed to create feedback", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}



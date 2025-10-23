namespace readytohelpapi.Feedback.Tests;

using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;
using readytohelpapi.Feedback.Tests.Fixtures;

/// <summary>
///  This class contains all unit tests related to the feedback service.
/// </summary>
public class TestFeedbackServiceTest
{
    private readonly Mock<IFeedbackRepository> mockRepo;
    private readonly IFeedbackService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFeedbackServiceTest"/> class.
    /// </summary>
    public TestFeedbackServiceTest()
    {
        mockRepo = new Mock<IFeedbackRepository>();
        service = new FeedbackServiceImpl(mockRepo.Object);
    }

    /// <summary>
    /// Tests the Create method with null feedback.
    /// </summary>
    [Fact]
    public void Create_NullFeedback_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => service.Create(null));
    }

    /// <summary>
    /// Tests the Create method when the user does not exist.
    /// </summary>
    [Fact]
    public void Create_UserNotFound_ThrowsArgumentException()
    {
        var fb = FeedbackFixture.Create(userId: 42, occurrenceId: 1);
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
        var fb = FeedbackFixture.Create(userId: 1, occurrenceId: 99);
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
        var input = FeedbackFixture.Create(userId: 1, occurrenceId: 2, isConfirmed: true);

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
        var sample = FeedbackFixture.Create(id: 1, userId: 1, occurrenceId: 2);
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
        var list = new List<Feedback> { FeedbackFixture.Create(id: 1), FeedbackFixture.Create(id: 2) };
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
        var expected = new List<Feedback> { FeedbackFixture.Create(id: 7, occurrenceId: 2) };
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
        var expected = new List<Feedback> { FeedbackFixture.Create(id: 9, userId: 1) };
        mockRepo.Setup(r => r.GetFeedbacksByUserId(1)).Returns(expected);

        var res = service.GetFeedbacksByUserId(1);

        Assert.Equal(expected, res);
        mockRepo.Verify(r => r.GetFeedbacksByUserId(1), Times.Once);
    }
}

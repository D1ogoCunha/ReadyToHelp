namespace readytohelpapi.Feedback.Controllers;

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;

/// <summary>
/// Controller for managing feedback.
/// </summary>
[ApiController]
[Route("api/feedback")]
public class FeedbackApiController : ControllerBase
{
    private readonly IFeedbackService service;

    /// <summary>
    /// Constructor for FeedbackApiController.
    /// </summary>
    /// <param name="service">The feedback service.</param>
    /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
    public FeedbackApiController(IFeedbackService service)
    {
        this.service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Create a new feedback.
    /// </summary>
    [HttpPost]
    public IActionResult Create([FromBody] Feedback feedback)
    {
        try
        {
            var created = service.Create(feedback);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            if (
                !string.IsNullOrEmpty(ex.Message)
                && ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            )
                return NotFound(new { error = ex.Message });

            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all feedbacks.
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            var list = service.GetAllFeedbacks();
            if (list == null || !list.Any())
                return NotFound(new { error = "No feedbacks found" });

            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get feedbacks by userId.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
    {
        try
        {
            var list = service.GetFeedbacksByUserId(userId);
            if (list == null || !list.Any())
                return NotFound(new { error = $"No feedbacks found for user {userId}" });

            return Ok(list);
        }
        catch (ArgumentException ex)
        {
            if (
                !string.IsNullOrEmpty(ex.Message)
                && ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            )
                return NotFound(new { error = ex.Message });

            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get feedback by feedbackId.
    /// </summary>
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            var fb = service.GetFeedbackById(id);
            if (fb == null)
                return NotFound(new { error = $"Feedback with id {id} does not exist" });

            return Ok(fb);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get feedbacks by occurrenceId.
    /// </summary>
    [HttpGet("occurrence/{occurrenceId:int}")]
    public IActionResult GetByOccurrenceId(int occurrenceId)
    {
        try
        {
            var list = service.GetFeedbacksByOccurrenceId(occurrenceId);
            if (list == null || !list.Any())
                return NotFound(
                    new { error = $"No feedbacks found for occurrence {occurrenceId}" }
                );

            return Ok(list);
        }
        catch (ArgumentException ex)
        {
            if (
                !string.IsNullOrEmpty(ex.Message)
                && ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            )
                return NotFound(new { error = ex.Message });

            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

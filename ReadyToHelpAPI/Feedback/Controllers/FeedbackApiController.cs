namespace readytohelpapi.Feedback.Controllers;

using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Feedback.Models;
using readytohelpapi.Feedback.Services;

/// <summary>
///   Provides API endpoints for managing feedbacks.
/// </summary>
[ApiController]
[Route("api/feedback")]
public class FeedbackApiController : ControllerBase
{
    private readonly IFeedbackService service;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FeedbackApiController"/> class.
    /// </summary>
    /// <param name="service">The feedback service.</param>
    public FeedbackApiController(IFeedbackService service)
    {
        this.service = service;
    }

    /// <summary>
    ///   Creates a new feedback.
    /// </summary>
    /// <param name="feedback">The feedback to create.</param>
    /// <returns>The created feedback.</returns>
    [HttpPost]
    public IActionResult Create([FromBody] Feedback feedback)
    {
        if (feedback == null)
        {
            return BadRequest(new { error = "Feedback is null" });
        }
        try
        {
            var created = service.Create(feedback);
            // Em vez de CreatedAtAction(GetById,...)
            return StatusCode(StatusCodes.Status201Created, created);
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

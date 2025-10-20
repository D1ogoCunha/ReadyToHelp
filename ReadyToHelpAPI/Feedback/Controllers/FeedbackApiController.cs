namespace readytohelpapi.Feedback.Controllers;

using System;
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
    /// Get feedback by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var fb = service.GetFeedbackById(id);
        if (fb == null)
            return NotFound();
        return Ok(fb);
    }

    /// <summary>
    /// Get feedbacks for a given occurrence.
    /// </summary>
    [HttpGet("occurrence/{occurrenceId:int}")]
    public IActionResult GetByOccurrenceId(int occurrenceId)
    {
        var list = service.GetFeedbacksByOccurrenceId(occurrenceId);
        return Ok(list);
    }
}

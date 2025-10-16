namespace readytohelpapi.Occurrence.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.Occurrence.Services;
using System;
using System.Collections.Generic;

/// <summary>
///   Provides API endpoints for managing occurrences.
/// </summary>
[ApiController]
[Route("api/occurrence")]
public class OccurrenceApiController : ControllerBase
{
    private readonly IOccurrenceService occurrenceService;

    public OccurrenceApiController(IOccurrenceService occurrenceService)
    {
        this.occurrenceService = occurrenceService;
    }

    /// <summary>
    /// Creates a new occurrence. Only ADMIN can call.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public ActionResult Create([FromBody] Occurrence occurrence)
    {
        if (occurrence is null) return BadRequest(new { error = "Occurrence payload is required." });

        try
        {
            var created = occurrenceService.Create(occurrence);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Gets an occurrence by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult GetById(int id)
    {
        try
        {
            var occurrence = occurrenceService.GetOccurrenceById(id);
            if (occurrence == null) return NotFound();
            return Ok(occurrence);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lists occurrences with pagination, sorting and filtering.
    /// </summary>
    [HttpGet]
    public ActionResult<List<Occurrence>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Title",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string filter = "")
    {
        try
        {
            var occurrences = occurrenceService.GetAllOccurrences(pageNumber, pageSize, sortBy, sortOrder, filter);
            return Ok(occurrences);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Gets occurrences by title.
    /// </summary>
    [HttpGet("title/{title}")]
    public ActionResult<List<Occurrence>> GetByTitle(string title)
    {
        try
        {
            var occurrences = occurrenceService.GetOccurrenceByTitle(title);
            return Ok(occurrences);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Gets occurrences by type.
    /// </summary>
    [HttpGet("type/{type}")]
    public ActionResult<List<Occurrence>> GetByType(OccurrenceType type)
    {
        try
        {
            var occurrences = occurrenceService.GetOccurrencesByType(type);
            return Ok(occurrences);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Gets occurrences by priority.
    /// </summary>
    [HttpGet("priority/{priority}")]
    public ActionResult<List<Occurrence>> GetByPriority(PriorityLevel priority)
    {
        try
        {
            var occurrences = occurrenceService.GetOccurrencesByPriority(priority);
            return Ok(occurrences);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Gets all active occurrences.
    /// </summary>
    [HttpGet("active")]
    public ActionResult<List<Occurrence>> GetAllActive()
    {
        try
        {
            var occurrences = occurrenceService.GetAllActiveOccurrences();
            return Ok(occurrences);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }
}
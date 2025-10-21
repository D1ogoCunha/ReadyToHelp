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
    //[Authorize(Roles = "ADMIN")]
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
    public ActionResult<List<OccurrenceMapDto>> GetAllActive(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] OccurrenceType? type = null,
        [FromQuery] PriorityLevel? priority = null,
        [FromQuery] int? responsibleEntityId = null)
    {
        try
        {
            var occurrences = occurrenceService.GetAllActiveOccurrences(pageNumber, pageSize, type, priority, responsibleEntityId);

            if (occurrences == null || occurrences.Count == 0)
                return NotFound(new { error = "no_active_occurrences" });

            var result = occurrences
                .Where(o => o.Location != null)
                .Select(o => new OccurrenceMapDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Type = o.Type,
                    Latitude = o.Location!.Latitude,
                    Longitude = o.Location!.Longitude,
                    Status = o.Status,
                    Priority = o.Priority
                })
                .ToList();

            if (result.Count == 0)
                return NotFound(new { error = "no_active_occurrences_with_valid_location" });

            return Ok(result);
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
    /// Updates an occurrence by ID.
    /// </summary>
    [HttpPut]
    [Route("")]
    public IActionResult Update([FromBody] Occurrence occurrence)
    {
        if (occurrence == null)
            return BadRequest("Occurrence cannot be null.");

        try
        {
            var updatedOccurrence = occurrenceService.Update(occurrence);
            return Ok(updatedOccurrence);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }

    /// <summary>
    /// Deletes an occurrence by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid occurrence ID.");

        try
        {
            var deletedOccurrence = occurrenceService.Delete(id);
            return Ok(deletedOccurrence);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }
}
namespace readytohelpapi.Report.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Models;
using readytohelpapi.Report.DTOs;

/// <summary>
/// Provides API endpoints for managing reports.
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportApiController : ControllerBase
{
    private readonly IReportService reportService;
    private readonly IReportRepository reportRepository;
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportApiController"/> class.
    /// </summary>
    /// <param name="reportService">The report service.</param>
    /// <param name="reportRepository">The report repository.</param>
    /// <param name="context">The database context.</param>
    public ReportApiController(
        IReportService reportService,
        IReportRepository reportRepository,
        AppDbContext context)
    {
        this.reportService = reportService;
        this.reportRepository = reportRepository;
        this.context = context;
    }

    /// <summary>
    /// Creates a new report.
    /// </summary>
    /// <param name="dto">The data transfer object containing report details.</param>
    /// <returns>The created report along with occurrence details.</returns>
    [HttpPost]
    public IActionResult Create([FromBody] CreateReportDto? dto)
    {
        if (dto == null)
            return BadRequest(new { error = "invalid_request", detail = "Request body is required." });

        try
        {
            var report = new Report
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Priority = dto.Priority,
                UserId = dto.UserId,
                Location = new GeoPoint.Models.GeoPoint
                {
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude
                }
            };

            var (createdReport, occurrence) = reportService.Create(report);

            ResponsibleEntityContactDto? responsibleDto = null;
            if (occurrence.ResponsibleEntityId > 0)
            {
                var entity = context.ResponsibleEntities
                    .AsNoTracking()
                    .FirstOrDefault(re => re.Id == occurrence.ResponsibleEntityId);

                if (entity != null)
                {
                    responsibleDto = new ResponsibleEntityContactDto
                    {
                        Name = entity.Name,
                        Email = entity.Email,
                        Address = entity.Address,
                        ContactPhone = entity.ContactPhone
                    };
                }
            }

            var response = new ReportResponseDto
            {
                ReportId = createdReport.Id,
                OccurrenceId = occurrence.Id,
                OccurrenceStatus = occurrence.Status,
                ResponsibleEntity = responsibleDto
            };

            return CreatedAtAction(nameof(GetById), new { id = createdReport.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "validation_error", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_server_error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a report by its ID.
    /// </summary>
    /// <param name="id">The ID of the report.</param>
    /// <returns>The report if found; otherwise, a not found response.</returns>
    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        if (id <= 0) return BadRequest("Invalid id.");
        var report = reportRepository.GetById(id);
        if (report is null) return NotFound();
        return Ok(report);
    }
}
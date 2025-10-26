namespace readytohelpapi.Report.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.Common.Data;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Models;
using readytohelpapi.Report.DTOs;

[ApiController]
[Route("api/reports")]
public class ReportApiController : ControllerBase
{
    private readonly IReportService reportService;
    private readonly IReportRepository reportRepository;
    private readonly AppDbContext context;

    public ReportApiController(
        IReportService reportService,
        IReportRepository reportRepository,
        AppDbContext context)
    {
        this.reportService = reportService;
        this.reportRepository = reportRepository;
        this.context = context;
    }

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

    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        if (id <= 0) return BadRequest("Invalid id.");
        var report = reportRepository.GetById(id);
        if (report is null) return NotFound();
        return Ok(report);
    }
}
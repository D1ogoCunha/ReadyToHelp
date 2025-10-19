namespace readytohelpapi.Report.Controllers;

using Microsoft.AspNetCore.Mvc;
using readytohelpapi.Report.Services;
using readytohelpapi.Report.Models;

[ApiController]
[Route("api/reports")]
public class ReportApiController : ControllerBase
{
    private readonly IReportService reportService;
    private readonly IReportRepository reportRepository;

    public ReportApiController(IReportService reportService, IReportRepository reportRepository)
    {
        this.reportService = reportService;
        this.reportRepository = reportRepository;
    }

    [HttpPost]
    public IActionResult Create([FromBody, Bind("Title,Description,UserId,Type,Priority,Location")] Report request)
    {
        if (request is null) return BadRequest("Report body is required.");

        try
        {
            var (createdReport, createdOccurrence) = reportService.Create(request);
            var response = new
            {
                reportId = createdReport.Id,
                occurrenceId = createdOccurrence.Id,
                report = createdReport,
                occurrence = createdOccurrence
            };

            return CreatedAtAction(nameof(GetById), new { id = createdReport.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
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
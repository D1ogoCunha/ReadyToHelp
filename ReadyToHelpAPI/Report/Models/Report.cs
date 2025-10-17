using GeoPointModel = readytohelpapi.GeoPoint.Models.GeoPoint;
using readytohelpapi.Occurrence.Models;

namespace readytohelpapi.Report.Models;

/// <summary>
///   Represents a report.
/// </summary>
public class Report
{
    /// <summary>
    ///   Gets or sets the unique identifier for the report.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///   Gets or sets the description of the report.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    ///   Gets or sets the date and time the report was created.
    /// </summary>
    public DateTime ReportDateTime { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the report is marked as duplicate.
    /// </summary>
    public bool IsDuplicate { get; set; }

    /// <summary>
    ///   Gets or sets the title of the occurrence on the report.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///   Gets or sets the status of the occurrence on the report.
    /// </summary>
    public OccurrenceStatus Status { get; set; }

    /// <summary>
    ///   Gets or sets the priority level of the occurrence on the report.
    /// </summary>
    public PriorityLevel Priority { get; set; }

    /// <summary>
    ///   Gets or sets the type of occurrence reported.
    /// </summary>
    public OccurrenceType Type { get; set; }

    /// <summary>
    ///   Gets or sets the user identifier associated with the report.
    /// </summary>
    public int UserId { get; set; }

    public GeoPointModel Location { get; set; }
}

namespace readytohelpapi.Report.Models;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.Models;

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
    public string Description { get; set; }

    /// <summary>
    ///   Gets or sets the date and time the report was created.
    /// </summary>
    public DateTime ReportDateTime { get; set; }

    /// <summary>
    ///   Gets or sets the title of the occurrence on the report.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///   Gets or sets the status of the occurrence on the report.
    /// </summary>
    public OccurrenceStatus Status { get; set; }

    /// <summary>
    ///   Gets or sets the type of occurrence reported.
    /// </summary>
    public OccurrenceType Type { get; set; }

    /// <summary>
    ///   Gets or sets the user identifier associated with the report.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    ///   Gets or sets the geographical location of the report.
    /// </summary>
    public GeoPoint Location { get; set; }
}

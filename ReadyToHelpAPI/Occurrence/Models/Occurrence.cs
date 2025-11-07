namespace readytohelpapi.Occurrence.Models;

using readytohelpapi.GeoPoint.Models;
using readytohelpapi.Occurrence.DTOs;

/// <summary>
///   Represents an occurrence.
/// </summary>
public class Occurrence
{
    /// <summary>
    ///   Gets or sets the unique identifier for the occurrence.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///   Gets or sets the title of the occurrence.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///   Gets or sets the description of the occurrence.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///   Gets or sets the type of the occurrence.
    /// </summary>
    public OccurrenceType Type { get; set; }

    /// <summary>
    ///   Gets or sets the status of the occurrence.
    /// </summary>
    public OccurrenceStatus Status { get; set; }

    /// <summary>
    ///  Gets or sets the priority level of the occurrence.
    /// </summary>
    public PriorityLevel Priority { get; set; }

    /// <summary>
    /// Gets or sets the proximity radius of the occurrence.
    /// </summary>
    public double ProximityRadius { get; set; }

    /// <summary>
    ///  Gets or sets the creation date and time of the occurrence.
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time of the occurrence.
    /// </summary>
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the report count of the occurrence.
    /// </summary>
    public int ReportCount { get; set; }

    /// <summary>
    /// Gets or sets the report identifier associated with the occurrence.
    /// </summary>
    public int? ReportId { get; set; }

    /// <summary>
    /// Gets or sets the responsible entity identifier associated with the occurrence.
    /// </summary>
    public int? ResponsibleEntityId { get; set; }

    /// <summary>
    /// Gets or sets the location of the occurrence.
    /// </summary>
    public GeoPoint Location { get; set; }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Occurrence" /> class.
    /// </summary>
    public Occurrence()
    {
        Title = string.Empty;
        Description = string.Empty;
        Location = new GeoPoint();
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Occurrence" /> class with specified parameters.
    /// Constructor that initializes all properties except CreationDateTime, which is set to current UTC time.
    /// </summary>
    /// <param name="id">The unique identifier for the occurrence.</param>
    /// <param name="title">The title of the occurrence.</param>
    /// <param name="description">The description of the occurrence.</param>
    /// <param name="type">The type of the occurrence.</param>
    /// <param name="status">The status of the occurrence.</param>
    /// <param name="priority">The priority level of the occurrence.</param>
    /// <param name="proximityRadius">The proximity radius of the occurrence.</param>
    /// <param name="endDateTime">The end date and time of the occurrence.</param>
    /// <param name="reportCount">The report count of the occurrence.</param>
    /// <param name="reportId">The report identifier associated with the occurrence.</param>
    /// <param name="responsibleEntityId">The responsible entity identifier associated with the occurrence.</param>
    public Occurrence(OccurrenceCreateDto dto)
    {
        Id = dto.Id;
        Title = dto.Title;
        Description = dto.Description;
        Type = dto.Type;
        Status = dto.Status;
        Priority = dto.Priority;
        ProximityRadius = dto.ProximityRadius;
        CreationDateTime = DateTime.UtcNow;
        EndDateTime = dto.EndDateTime;
        ReportCount = dto.ReportCount;
        ReportId = dto.ReportId;
        ResponsibleEntityId = dto.ResponsibleEntityId;
        Location = dto.Location;
    }
}

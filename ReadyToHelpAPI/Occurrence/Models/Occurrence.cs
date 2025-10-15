using System.ComponentModel.DataAnnotations;

namespace readytohelpapi.Occurrence.Models;

/// <summary>
///   Represents an occurrence.
/// </summary>

public class Occurrence
{

    /// <summary>
    ///  Initializes a new instance of the <see cref="Occurrence" /> class.
    ///  Default constructor that sets CreationDateTime to current UTC time and Status to ACTIVE.
    /// </summary>
    public Occurrence()
    {
        CreationDateTime = DateTime.UtcNow;
        Status = OccurrenceStatus.ACTIVE;
        ReportCount = 0;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Occurrence" /> class with specified parameters.
    /// Constructor that initializes all properties except CreationDateTime, which is set to current UTC time.
    /// </summary>
    /// <param name="id">The unique identifier for the occurrence.</param>
    /// <param name="title">The title of the occurrence.</param>
    /// <param name="description">The description of the occurrence.</param>
    /// <param name="location">The location of the occurrence.</param>
    /// <param name="type">The type of the occurrence.</param>
    /// <param name="status">The status of the occurrence.</param>
    /// <param name="priority">The priority level of the occurrence.</param>
    /// <param name="proximityRadius">The proximity radius of the occurrence.</param>
    /// <param name="endDateTime">The end date and time of the occurrence.</param>
    /// <param name="reportCount">The report count of the occurrence.</param>
    /// <param name="reportId">The report identifier associated with the occurrence.</param>
    /// <param name="responsibleEntityId">The responsible entity identifier associated with the occurrence.</param>
    public Occurrence(int id, string title, string description, /*string location,*/ OccurrenceType type, OccurrenceStatus status, PriorityLevel priority, double proximityRadius, DateTime endDateTime, int reportCount, int reportId, int responsibleEntityId)
    {
        Id = id;
        Title = title;
        Description = description;
        //Location = location;
        Type = type;
        Status = status;
        Priority = priority;
        ProximityRadius = proximityRadius;
        CreationDateTime = DateTime.UtcNow;
        EndDateTime = endDateTime;
        ReportCount = reportCount;
        ReportId = reportId;
        ResponsibleEntityId = responsibleEntityId;
    }

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
    ///   Gets or sets the location of the occurrence.
    /// </summary>
    //public string Location { get; set; }

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
    public int ReportId { get; set; }

    /// <summary>
    /// Gets or sets the responsible entity identifier associated with the occurrence.
    /// </summary>
    public int ResponsibleEntityId { get; set; }
}

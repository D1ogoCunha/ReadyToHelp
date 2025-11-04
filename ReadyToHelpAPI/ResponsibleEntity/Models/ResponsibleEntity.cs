namespace readytohelpapi.ResponsibleEntity.Models;

using NetTopologySuite.Geometries;

/// <summary>
/// Represents a responsible entity.
/// </summary>
public class ResponsibleEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the responsible entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the responsible entity.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email of the responsible entity.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the address of the responsible entity.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number of the responsible entity.
    /// </summary>
    public int ContactPhone { get; set; }

    /// <summary>
    /// Gets or sets the type of the responsible entity.
    /// </summary>
    public ResponsibleEntityType Type { get; set; }

    /// <summary>
    /// Gets or sets the geographical area covered by the responsible entity.
    /// </summary>
    public Geometry? GeoArea { get; set; }
}

namespace readytohelpapi.ResponsibleEntity.Models;

using NetTopologySuite.Geometries;

public class ResponsibleEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public int ContactPhone { get; set; }
    public ResponsibleEntityType Type { get; set; }
    public Geometry? GeoArea { get; set; }
}
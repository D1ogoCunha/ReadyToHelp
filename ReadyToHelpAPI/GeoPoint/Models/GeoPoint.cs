using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace readytohelpapi.GeoPoint.Models;

/// <summary>
///  Represents a geographical point with latitude and longitude.
/// </summary>
[Owned]
public class GeoPoint
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GeoPoint"/> class.
    /// </summary>
    public GeoPoint() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeoPoint"/> class with specified latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude of the geographical point.</param>
    /// <param name="longitude">The longitude of the geographical point.</param>
    public GeoPoint(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude < -180 || longitude > 180) throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = Math.Round(latitude, 6, MidpointRounding.AwayFromZero);
        Longitude = Math.Round(longitude, 6, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    ///    Gets or sets the latitude of the geographical point.
    /// </summary>
    [Range(-90, 90)]
    public double Latitude { get; set; }

    /// <summary>
    ///    Gets or sets the longitude of the geographical point.
    /// </summary>
    [Range(-180, 180)]
    public double Longitude { get; set; }
}
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace readytohelpapi.GeoPoint.Models;

[Owned]
public class GeoPoint
{
    public GeoPoint() { }

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude < -180 || longitude > 180) throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = Math.Round(latitude, 6, MidpointRounding.AwayFromZero);
        Longitude = Math.Round(longitude, 6, MidpointRounding.AwayFromZero);
    }

    [Range(-90, 90)]
    public double Latitude { get; private set; }

    [Range(-180, 180)]
    public double Longitude { get; private set; }
}
namespace readytohelpapi.GeoPoint.Miscellaneous;

/// <summary>
///  Provides geographical utility methods.
/// </summary>
public static class GeoUtils
{
    /// <summary>
    /// Calculates the distance in meters between two geographical points using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point.</param>
    /// <param name="lon1">Longitude of the first point.</param>
    /// <param name="lat2">Latitude of the second point.</param>
    /// <param name="lon2">Longitude of the second point.</param>
    /// <returns>The distance in meters between the two points.</returns>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371_000d;
        var φ1 = ToRad(lat1);
        var φ2 = ToRad(lat2);
        var Δφ = ToRad(lat2 - lat1);
        var Δλ = ToRad(lon2 - lon1);

        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double ToRad(double value) => (Math.PI / 180d) * value;
}
using GeoCoordinatePortable;

namespace readytohelpapi.GeoPoint.Miscellaneous;

public static class GeoUtils
{
public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
{
    var a = new GeoCoordinate(lat1, lon1);
    var b = new GeoCoordinate(lat2, lon2);
    return a.GetDistanceTo(b);
}

    private static double ToRad(double value) => (Math.PI / 180) * value;
}
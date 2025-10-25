namespace readytohelpapi.GeoPoint.Miscellaneous;

using NetTopologySuite;
using NetTopologySuite.Geometries;

public static class GeoPolygonUtils
{
    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static Polygon BuildPolygon(IEnumerable<(double lat, double lon)> latLonRing)
    {
        var coords = latLonRing.Select(p => new Coordinate(p.lon, p.lat)).ToList();
        if (coords.Count < 4) throw new ArgumentException("Polygon needs at least 4 coordinates (closed ring).");
        if (!coords.First().Equals2D(coords.Last()))
            coords.Add(new Coordinate(coords[0].X, coords[0].Y));

        var shell = new LinearRing(coords.ToArray());
        return new Polygon(shell) { SRID = 4326 };
    }

    public static Point BuildPoint(double lat, double lon) =>
        new Point(lon, lat) { SRID = 4326 };
}
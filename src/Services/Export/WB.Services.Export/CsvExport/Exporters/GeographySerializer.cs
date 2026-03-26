using System;
using System.Globalization;
using System.Text;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class GeographySerializer : IGeographySerializer
    {
        private const double CoordinateEqualityTolerance = 1e-10;

        /// <summary>
        /// Serializes a geography answer according to the specified export format.
        /// WGS84 coordinates are parsed from <paramref name="area"/>.Coordinates
        /// (semicolon-separated "lon,lat" pairs produced by GeometryHelper.GetProjectedCoordinates).
        /// The geometry type (Point/Polyline/Polygon/Multipoint) comes from the questionnaire question definition.
        /// Falls back to the legacy coordinates string when input is invalid or empty.
        /// </summary>
        public string Serialize(Area? area, GeometryType? geometryType, GeographyExportFormat format)
        {
            if (area == null)
                return string.Empty;

            if (format == GeographyExportFormat.Legacy)
                return area.Coordinates ?? string.Empty;

            var coords = ParseCoordinates(area.Coordinates);
            if (coords == null || coords.Length == 0)
                return area.Coordinates ?? string.Empty;

            if (geometryType == null)
                return area.Coordinates ?? string.Empty;

            try
            {
                var result = format == GeographyExportFormat.Wkt
                    ? ToWkt(geometryType.Value, coords)
                    : ToGeoJson(geometryType.Value, coords);

                return string.IsNullOrEmpty(result) ? (area.Coordinates ?? string.Empty) : result;
            }
            catch
            {
                return area.Coordinates ?? string.Empty;
            }
        }

        /// <summary>
        /// Parses semicolon-separated "lon,lat" pairs from the Coordinates string.
        /// Returns null on parse failure; empty array for empty/whitespace input.
        /// </summary>
        private static GeoCoordinate[]? ParseCoordinates(string? coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
                return Array.Empty<GeoCoordinate>();

            var parts = coordinates.Split(';');
            var result = new GeoCoordinate[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var xy = parts[i].Split(',');
                if (xy.Length < 2)
                    return null;
                if (!double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                    return null;
                if (!double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                    return null;
                result[i] = new GeoCoordinate(lon, lat);
            }
            return result;
        }

        private static string FormatWktCoord(GeoCoordinate c)
            => c.Lon.ToString("G", CultureInfo.InvariantCulture) + " " + c.Lat.ToString("G", CultureInfo.InvariantCulture);

        private static string FormatGeoJsonCoord(GeoCoordinate c)
            => c.Lon.ToString("G", CultureInfo.InvariantCulture) + "," + c.Lat.ToString("G", CultureInfo.InvariantCulture);

        private static string ToWkt(GeometryType geometryType, GeoCoordinate[] coords)
        {
            switch (geometryType)
            {
                case GeometryType.Point:
                    if (coords.Length < 1) return string.Empty;
                    return $"POINT({FormatWktCoord(coords[0])})";

                case GeometryType.Multipoint:
                    if (coords.Length == 0) return string.Empty;
                    var mpSb = new StringBuilder("MULTIPOINT(");
                    for (int i = 0; i < coords.Length; i++)
                    {
                        if (i > 0) mpSb.Append(',');
                        mpSb.Append('(');
                        mpSb.Append(FormatWktCoord(coords[i]));
                        mpSb.Append(')');
                    }
                    mpSb.Append(')');
                    return mpSb.ToString();

                case GeometryType.Polyline:
                    if (coords.Length < 2) return string.Empty;
                    return "LINESTRING(" + FormatWktCoordList(coords) + ")";

                case GeometryType.Polygon:
                    if (coords.Length < 3) return string.Empty;
                    return "POLYGON((" + FormatWktCoordList(EnsureClosed(coords)) + "))";

                default:
                    return string.Empty;
            }
        }

        private static string ToGeoJson(GeometryType geometryType, GeoCoordinate[] coords)
        {
            switch (geometryType)
            {
                case GeometryType.Point:
                    if (coords.Length < 1) return string.Empty;
                    return $"{{\"type\":\"Point\",\"coordinates\":[{FormatGeoJsonCoord(coords[0])}]}}";

                case GeometryType.Multipoint:
                    if (coords.Length == 0) return string.Empty;
                    var mpSb = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":[");
                    for (int i = 0; i < coords.Length; i++)
                    {
                        if (i > 0) mpSb.Append(',');
                        mpSb.Append($"[{FormatGeoJsonCoord(coords[i])}]");
                    }
                    mpSb.Append("]}");
                    return mpSb.ToString();

                case GeometryType.Polyline:
                    if (coords.Length < 2) return string.Empty;
                    return "{\"type\":\"LineString\",\"coordinates\":" + FormatGeoJsonCoordArray(coords) + "}";

                case GeometryType.Polygon:
                    if (coords.Length < 3) return string.Empty;
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonCoordArray(EnsureClosed(coords)) + "]}";

                default:
                    return string.Empty;
            }
        }

        private static string FormatWktCoordList(GeoCoordinate[] points)
        {
            var parts = new string[points.Length];
            for (int i = 0; i < points.Length; i++)
                parts[i] = FormatWktCoord(points[i]);
            return string.Join(",", parts);
        }

        private static string FormatGeoJsonCoordArray(GeoCoordinate[] points)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append($"[{FormatGeoJsonCoord(points[i])}]");
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static GeoCoordinate[] EnsureClosed(GeoCoordinate[] ring)
        {
            if (ring.Length < 2) return ring;
            var first = ring[0];
            var last = ring[ring.Length - 1];
            if (Math.Abs(first.Lon - last.Lon) > CoordinateEqualityTolerance ||
                Math.Abs(first.Lat - last.Lat) > CoordinateEqualityTolerance)
            {
                var closed = new GeoCoordinate[ring.Length + 1];
                Array.Copy(ring, closed, ring.Length);
                closed[ring.Length] = first;
                return closed;
            }
            return ring;
        }
    }
}

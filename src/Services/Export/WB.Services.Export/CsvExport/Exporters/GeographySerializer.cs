using System;
using System.Globalization;
using System.Text;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public static class GeographySerializer
    {
        private const double CoordinateEqualityTolerance = 1e-10;

        /// <summary>
        /// Serializes a geography answer according to the specified export format.
        /// WGS84 coordinates are parsed from <paramref name="area"/>.Coordinates
        /// (semicolon-separated "lon,lat" pairs produced by GeometryHelper.GetProjectedCoordinates).
        /// The geometry type (Point/Polyline/Polygon/Multipoint) comes from the questionnaire question definition.
        /// Falls back to the legacy coordinates string when input is invalid or empty.
        /// </summary>
        public static string Serialize(Area? area, GeometryType? geometryType, GeographyExportFormat format)
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
        private static (double lon, double lat)[]? ParseCoordinates(string? coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
                return Array.Empty<(double, double)>();

            var parts = coordinates.Split(';');
            var result = new (double lon, double lat)[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var xy = parts[i].Split(',');
                if (xy.Length < 2)
                    return null;
                if (!double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                    return null;
                if (!double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                    return null;
                result[i] = (lon, lat);
            }
            return result;
        }

        private static string FormatWktCoord(double lon, double lat)
            => lon.ToString("G", CultureInfo.InvariantCulture) + " " + lat.ToString("G", CultureInfo.InvariantCulture);

        private static string FormatGeoJsonCoord(double lon, double lat)
            => lon.ToString("G", CultureInfo.InvariantCulture) + "," + lat.ToString("G", CultureInfo.InvariantCulture);

        private static string ToWkt(GeometryType geometryType, (double lon, double lat)[] coords)
        {
            switch (geometryType)
            {
                case GeometryType.Point:
                    if (coords.Length < 1) return string.Empty;
                    return $"POINT({FormatWktCoord(coords[0].lon, coords[0].lat)})";

                case GeometryType.Multipoint:
                    if (coords.Length == 0) return string.Empty;
                    var mpSb = new StringBuilder("MULTIPOINT(");
                    for (int i = 0; i < coords.Length; i++)
                    {
                        if (i > 0) mpSb.Append(',');
                        mpSb.Append('(');
                        mpSb.Append(FormatWktCoord(coords[i].lon, coords[i].lat));
                        mpSb.Append(')');
                    }
                    mpSb.Append(')');
                    return mpSb.ToString();

                case GeometryType.Polyline:
                    if (coords.Length == 0) return string.Empty;
                    return "LINESTRING(" + FormatWktCoordList(coords) + ")";

                case GeometryType.Polygon:
                    if (coords.Length < 2) return string.Empty;
                    return "POLYGON((" + FormatWktCoordList(EnsureClosed(coords)) + "))";

                default:
                    return string.Empty;
            }
        }

        private static string ToGeoJson(GeometryType geometryType, (double lon, double lat)[] coords)
        {
            switch (geometryType)
            {
                case GeometryType.Point:
                    if (coords.Length < 1) return string.Empty;
                    return $"{{\"type\":\"Point\",\"coordinates\":[{FormatGeoJsonCoord(coords[0].lon, coords[0].lat)}]}}";

                case GeometryType.Multipoint:
                    if (coords.Length == 0) return string.Empty;
                    var mpSb = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":[");
                    for (int i = 0; i < coords.Length; i++)
                    {
                        if (i > 0) mpSb.Append(',');
                        mpSb.Append($"[{FormatGeoJsonCoord(coords[i].lon, coords[i].lat)}]");
                    }
                    mpSb.Append("]}");
                    return mpSb.ToString();

                case GeometryType.Polyline:
                    if (coords.Length == 0) return string.Empty;
                    return "{\"type\":\"LineString\",\"coordinates\":" + FormatGeoJsonCoordArray(coords) + "}";

                case GeometryType.Polygon:
                    if (coords.Length < 2) return string.Empty;
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonCoordArray(EnsureClosed(coords)) + "]}";

                default:
                    return string.Empty;
            }
        }

        private static string FormatWktCoordList((double lon, double lat)[] points)
        {
            var parts = new string[points.Length];
            for (int i = 0; i < points.Length; i++)
                parts[i] = FormatWktCoord(points[i].lon, points[i].lat);
            return string.Join(",", parts);
        }

        private static string FormatGeoJsonCoordArray((double lon, double lat)[] points)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append($"[{FormatGeoJsonCoord(points[i].lon, points[i].lat)}]");
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static (double lon, double lat)[] EnsureClosed((double lon, double lat)[] ring)
        {
            if (ring.Length < 2) return ring;
            var first = ring[0];
            var last = ring[ring.Length - 1];
            if (Math.Abs(first.lon - last.lon) > CoordinateEqualityTolerance ||
                Math.Abs(first.lat - last.lat) > CoordinateEqualityTolerance)
            {
                var closed = new (double lon, double lat)[ring.Length + 1];
                Array.Copy(ring, closed, ring.Length);
                closed[ring.Length] = first;
                return closed;
            }
            return ring;
        }
    }
}

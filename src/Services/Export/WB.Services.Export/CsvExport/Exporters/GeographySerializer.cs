using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class GeographySerializer : IGeographySerializer
    {
        // Tolerance used to detect whether the polygon ring's last point equals its first point (ring closure).
        private const double CoordinateEqualityTolerance = 1e-10;
        private readonly ILogger<GeographySerializer> logger;

        public GeographySerializer(ILogger<GeographySerializer> logger)
        {
            this.logger = logger;
        }

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

            var fallback = area.Coordinates ?? string.Empty;

            if (format == GeographyExportFormat.Legacy)
                return fallback;

            if (!TryParseCoordinates(area.Coordinates, out var coords))
                return fallback;
            if (coords == null || coords.Length == 0)
                return fallback;

            if (geometryType == null)
                return fallback;

            try
            {
                string result;
                switch (format)
                {
                    case GeographyExportFormat.Wkt:
                        result = ToWkt(geometryType.Value, coords);
                        break;
                    case GeographyExportFormat.GeoJson:
                        result = ToGeoJson(geometryType.Value, coords);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, $"Unknown geography export format: {format}");
                }

                return string.IsNullOrEmpty(result) ? fallback : result;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogError(ex, "Unknown geography export format: {Format}", format);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to serialize geography answer with format {Format}", format);
                return fallback;
            }
        }

        /// <summary>
        /// Tries to parse semicolon-separated "lon,lat" pairs from the Coordinates string.
        /// Returns <c>true</c> with an empty array for empty/whitespace input.
        /// Returns <c>false</c> with <c>result = null</c> on parse failure.
        /// </summary>
        private static bool TryParseCoordinates(string? coordinates, out GeoCoordinate[]? result)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
            {
                result = Array.Empty<GeoCoordinate>();
                return true;
            }

            var parts = coordinates.Split(';');
            var coords = new GeoCoordinate[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var xy = parts[i].Split(',');
                if (xy.Length < 2)
                {
                    result = null;
                    return false;
                }
                if (!double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) ||
                    !double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                {
                    result = null;
                    return false;
                }
                coords[i] = new GeoCoordinate(lon, lat);
            }
            result = coords;
            return true;
        }

        private static string FormatWktCoord(GeoCoordinate c)
            => c.Lon.ToString("R", CultureInfo.InvariantCulture) + " " + c.Lat.ToString("R", CultureInfo.InvariantCulture);

        private static string FormatGeoJsonCoord(GeoCoordinate c)
            => c.Lon.ToString("R", CultureInfo.InvariantCulture) + "," + c.Lat.ToString("R", CultureInfo.InvariantCulture);

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
                    return "POLYGON((" + FormatWktCoordList(CloseRing(coords)) + "))";

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
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonCoordArray(CloseRing(coords)) + "]}";

                default:
                    return string.Empty;
            }
        }

        private static string FormatWktCoordList(GeoCoordinate[] points)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(FormatWktCoord(points[i]));
            }
            return sb.ToString();
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

        private static GeoCoordinate[] CloseRing(GeoCoordinate[] ring)
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

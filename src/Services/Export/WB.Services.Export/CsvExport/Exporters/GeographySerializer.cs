using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;

namespace WB.Services.Export.CsvExport.Exporters
{
    /// <summary>
    /// Esri JSON geometry model. Used for geometry type and structure detection only.
    /// Actual WGS84 coordinates are sourced from <see cref="Area.Coordinates"/>.
    /// </summary>
    public class EsriGeometry
    {
        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }

        /// <summary>Points array for Multipoint. Each element is [x, y, ...].</summary>
        [JsonProperty("points")]
        public double[][]? Points { get; set; }

        /// <summary>Paths array for Polyline. Each path is an array of [x, y, ...] points.</summary>
        [JsonProperty("paths")]
        public double[][][]? Paths { get; set; }

        /// <summary>Rings array for Polygon. Each ring is an array of [x, y, ...] points.</summary>
        [JsonProperty("rings")]
        public double[][][]? Rings { get; set; }
    }

    public static class GeographySerializer
    {
        private const double CoordinateEqualityTolerance = 1e-10;

        /// <summary>
        /// Serializes a geography answer according to the specified export format.
        /// WGS84 coordinates are sourced from <paramref name="area"/>.Coordinates
        /// (semicolon-separated "lon,lat" pairs); <paramref name="area"/>.Geometry (Esri JSON)
        /// is used only for geometry type and structure.
        /// Falls back to the legacy coordinates string when input is invalid or empty.
        /// </summary>
        public static string Serialize(Area? area, GeographyExportFormat format)
        {
            if (area == null)
                return string.Empty;

            if (format == GeographyExportFormat.Legacy)
                return area.Coordinates ?? string.Empty;

            var coords = ParseCoordinates(area.Coordinates);
            if (coords == null || coords.Length == 0)
                return area.Coordinates ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(area.Geometry))
            {
                try
                {
                    var geometry = JsonConvert.DeserializeObject<EsriGeometry>(area.Geometry);
                    if (geometry != null)
                    {
                        var result = format == GeographyExportFormat.Wkt
                            ? ToWkt(geometry, coords)
                            : ToGeoJson(geometry, coords);

                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                }
                catch
                {
                    // fall through to legacy
                }
            }

            return area.Coordinates ?? string.Empty;
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

        private static string ToWkt(EsriGeometry geometry, (double lon, double lat)[] coords)
        {
            // Point
            if (geometry.X.HasValue && geometry.Y.HasValue)
            {
                if (coords.Length < 1) return string.Empty;
                return $"POINT({FormatWktCoord(coords[0].lon, coords[0].lat)})";
            }

            // Multipoint
            if (geometry.Points != null)
            {
                if (coords.Length == 0) return string.Empty;
                var sb = new StringBuilder("MULTIPOINT(");
                for (int i = 0; i < coords.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append('(');
                    sb.Append(FormatWktCoord(coords[i].lon, coords[i].lat));
                    sb.Append(')');
                }
                sb.Append(')');
                return sb.ToString();
            }

            // Polyline
            if (geometry.Paths is { Length: > 0 })
            {
                var pathCoords = SplitByPartCounts(coords, geometry.Paths);
                if (pathCoords == null) return string.Empty;

                if (pathCoords.Length == 1)
                {
                    return "LINESTRING(" + FormatWktRing(pathCoords[0]) + ")";
                }
                else
                {
                    var sb = new StringBuilder("MULTILINESTRING(");
                    for (int i = 0; i < pathCoords.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('(');
                        sb.Append(FormatWktRing(pathCoords[i]));
                        sb.Append(')');
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            // Polygon
            if (geometry.Rings is { Length: > 0 })
            {
                var ringCoords = SplitByPartCounts(coords, geometry.Rings);
                if (ringCoords == null) return string.Empty;

                if (ringCoords.Length == 1)
                {
                    return "POLYGON((" + FormatWktRing(EnsureClosed(ringCoords[0])) + "))";
                }
                else
                {
                    var sb = new StringBuilder("MULTIPOLYGON(");
                    for (int i = 0; i < ringCoords.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("((");
                        sb.Append(FormatWktRing(EnsureClosed(ringCoords[i])));
                        sb.Append("))");
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        private static string ToGeoJson(EsriGeometry geometry, (double lon, double lat)[] coords)
        {
            // Point
            if (geometry.X.HasValue && geometry.Y.HasValue)
            {
                if (coords.Length < 1) return string.Empty;
                return $"{{\"type\":\"Point\",\"coordinates\":[{FormatGeoJsonCoord(coords[0].lon, coords[0].lat)}]}}";
            }

            // Multipoint
            if (geometry.Points != null)
            {
                if (coords.Length == 0) return string.Empty;
                var sb = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":[");
                for (int i = 0; i < coords.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append($"[{FormatGeoJsonCoord(coords[i].lon, coords[i].lat)}]");
                }
                sb.Append("]}");
                return sb.ToString();
            }

            // Polyline
            if (geometry.Paths is { Length: > 0 })
            {
                var pathCoords = SplitByPartCounts(coords, geometry.Paths);
                if (pathCoords == null) return string.Empty;

                if (pathCoords.Length == 1)
                {
                    return "{\"type\":\"LineString\",\"coordinates\":" + FormatGeoJsonRing(pathCoords[0]) + "}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiLineString\",\"coordinates\":[");
                    for (int i = 0; i < pathCoords.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(FormatGeoJsonRing(pathCoords[i]));
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            // Polygon
            if (geometry.Rings is { Length: > 0 })
            {
                var ringCoords = SplitByPartCounts(coords, geometry.Rings);
                if (ringCoords == null) return string.Empty;

                if (ringCoords.Length == 1)
                {
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonRing(EnsureClosed(ringCoords[0])) + "]}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiPolygon\",\"coordinates\":[");
                    for (int i = 0; i < ringCoords.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('[');
                        sb.Append(FormatGeoJsonRing(EnsureClosed(ringCoords[i])));
                        sb.Append(']');
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Splits flat WGS84 coordinate array into segments using point counts from Esri JSON parts
        /// (rings or paths). Returns null if total count doesn't match the flat coordinate count.
        /// </summary>
        private static (double lon, double lat)[][]? SplitByPartCounts(
            (double lon, double lat)[] coords,
            double[][][] parts)
        {
            int total = 0;
            foreach (var part in parts)
                total += part.Length;

            if (total != coords.Length)
                return null;

            var result = new (double lon, double lat)[parts.Length][];
            int offset = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                int count = parts[i].Length;
                var segment = new (double lon, double lat)[count];
                Array.Copy(coords, offset, segment, 0, count);
                result[i] = segment;
                offset += count;
            }
            return result;
        }

        private static string FormatWktRing((double lon, double lat)[] points)
        {
            var parts = new string[points.Length];
            for (int i = 0; i < points.Length; i++)
                parts[i] = FormatWktCoord(points[i].lon, points[i].lat);
            return string.Join(",", parts);
        }

        private static string FormatGeoJsonRing((double lon, double lat)[] points)
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

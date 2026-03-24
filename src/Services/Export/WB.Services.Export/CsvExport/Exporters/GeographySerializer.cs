using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;

namespace WB.Services.Export.CsvExport.Exporters
{
    /// <summary>
    /// Esri JSON geometry model. Coordinates are in WGS84 (lon/lat).
    /// </summary>
    public class EsriGeometry
    {
        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }

        [JsonProperty("points")]
        public double[][]? Points { get; set; }

        [JsonProperty("paths")]
        public double[][][]? Paths { get; set; }

        [JsonProperty("rings")]
        public double[][][]? Rings { get; set; }
    }

    public static class GeographySerializer
    {
        private const double CoordinateEqualityTolerance = 1e-10;

        /// <summary>
        /// Serializes a geography answer according to the specified export format.
        /// </summary>
        public static string Serialize(Area? area, GeographyExportFormat format)
        {
            if (area == null)
                return string.Empty;

            if (format == GeographyExportFormat.Legacy)
                return area.Coordinates ?? string.Empty;

            if (string.IsNullOrWhiteSpace(area.Geometry))
                return area.Coordinates ?? string.Empty;

            try
            {
                var geometry = JsonConvert.DeserializeObject<EsriGeometry>(area.Geometry);
                if (geometry == null)
                    return area.Coordinates ?? string.Empty;

                var result = format == GeographyExportFormat.Wkt
                    ? ToWkt(geometry)
                    : ToGeoJson(geometry);

                return string.IsNullOrEmpty(result) ? (area.Coordinates ?? string.Empty) : result;
            }
            catch
            {
                return area.Coordinates ?? string.Empty;
            }
        }

        private static string FormatCoord(double x, double y)
            => x.ToString("G", CultureInfo.InvariantCulture) + " " + y.ToString("G", CultureInfo.InvariantCulture);

        private static string ToWkt(EsriGeometry geometry)
        {
            if (geometry.X.HasValue && geometry.Y.HasValue)
            {
                return $"POINT({FormatCoord(geometry.X.Value, geometry.Y.Value)})";
            }

            if (geometry.Points is { Length: > 0 })
            {
                var sb = new StringBuilder("MULTIPOINT(");
                for (int i = 0; i < geometry.Points.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    var pt = geometry.Points[i];
                    if (pt.Length < 2) continue;
                    sb.Append('(');
                    sb.Append(FormatCoord(pt[0], pt[1]));
                    sb.Append(')');
                }
                sb.Append(')');
                return sb.ToString();
            }

            if (geometry.Paths is { Length: > 0 })
            {
                if (geometry.Paths.Length == 1)
                {
                    return "LINESTRING(" + FormatWktRing(geometry.Paths[0]) + ")";
                }
                else
                {
                    var sb = new StringBuilder("MULTILINESTRING(");
                    for (int i = 0; i < geometry.Paths.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('(');
                        sb.Append(FormatWktRing(geometry.Paths[i]));
                        sb.Append(')');
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            if (geometry.Rings is { Length: > 0 })
            {
                if (geometry.Rings.Length == 1)
                {
                    return "POLYGON((" + FormatWktRing(EnsureClosed(geometry.Rings[0])) + "))";
                }
                else
                {
                    var sb = new StringBuilder("MULTIPOLYGON(");
                    for (int i = 0; i < geometry.Rings.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("((");
                        sb.Append(FormatWktRing(EnsureClosed(geometry.Rings[i])));
                        sb.Append("))");
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        private static string ToGeoJson(EsriGeometry geometry)
        {
            if (geometry.X.HasValue && geometry.Y.HasValue)
            {
                return $"{{\"type\":\"Point\",\"coordinates\":[{FormatGeoJsonCoord(geometry.X.Value, geometry.Y.Value)}]}}";
            }

            if (geometry.Points is { Length: > 0 })
            {
                var sb = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":[");
                for (int i = 0; i < geometry.Points.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    var pt = geometry.Points[i];
                    if (pt.Length < 2) continue;
                    sb.Append($"[{FormatGeoJsonCoord(pt[0], pt[1])}]");
                }
                sb.Append("]}");
                return sb.ToString();
            }

            if (geometry.Paths is { Length: > 0 })
            {
                if (geometry.Paths.Length == 1)
                {
                    return "{\"type\":\"LineString\",\"coordinates\":" + FormatGeoJsonRing(geometry.Paths[0]) + "}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiLineString\",\"coordinates\":[");
                    for (int i = 0; i < geometry.Paths.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(FormatGeoJsonRing(geometry.Paths[i]));
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            if (geometry.Rings is { Length: > 0 })
            {
                if (geometry.Rings.Length == 1)
                {
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonRing(EnsureClosed(geometry.Rings[0])) + "]}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiPolygon\",\"coordinates\":[");
                    for (int i = 0; i < geometry.Rings.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('[');
                        sb.Append(FormatGeoJsonRing(EnsureClosed(geometry.Rings[i])));
                        sb.Append(']');
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        private static string FormatGeoJsonCoord(double x, double y)
            => x.ToString("G", CultureInfo.InvariantCulture) + "," + y.ToString("G", CultureInfo.InvariantCulture);

        private static string FormatWktRing(double[][] points)
        {
            var parts = new string[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var pt = points[i];
                parts[i] = pt.Length >= 2 ? FormatCoord(pt[0], pt[1]) : "0 0";
            }
            return string.Join(",", parts);
        }

        private static string FormatGeoJsonRing(double[][] points)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) sb.Append(',');
                var pt = points[i];
                if (pt.Length >= 2)
                    sb.Append($"[{FormatGeoJsonCoord(pt[0], pt[1])}]");
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static double[][] EnsureClosed(double[][] ring)
        {
            if (ring.Length < 2) return ring;
            var first = ring[0];
            var last = ring[ring.Length - 1];
            if (first.Length >= 2 && last.Length >= 2 &&
                (System.Math.Abs(first[0] - last[0]) > CoordinateEqualityTolerance ||
                 System.Math.Abs(first[1] - last[1]) > CoordinateEqualityTolerance))
            {
                var closed = new double[ring.Length + 1][];
                ring.CopyTo(closed, 0);
                closed[ring.Length] = first;
                return closed;
            }
            return ring;
        }
    }
}

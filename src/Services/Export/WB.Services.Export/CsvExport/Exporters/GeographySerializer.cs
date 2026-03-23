using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using WB.Services.Export.Models;

namespace WB.Services.Export.CsvExport.Exporters
{
    public static class GeographySerializer
    {
        private const double EarthRadiusMeters = 6378137.0;

        /// <summary>
        /// Serializes a geography answer according to the specified export format.
        /// </summary>
        /// <param name="geometry">Esri JSON geometry string (Web Mercator).</param>
        /// <param name="coordinatesLegacy">Legacy semicolon-separated coordinate string (WGS84).</param>
        /// <param name="format">Target export format.</param>
        /// <returns>Serialized geometry string or empty string if input is empty/invalid.</returns>
        public static string Serialize(string? geometry, string? coordinatesLegacy, GeographyExportFormat format)
        {
            if (format == GeographyExportFormat.Legacy)
                return coordinatesLegacy ?? string.Empty;

            if (string.IsNullOrWhiteSpace(geometry))
                return coordinatesLegacy ?? string.Empty;

            try
            {
                var json = JObject.Parse(geometry);
                return format == GeographyExportFormat.Wkt
                    ? ToWkt(json)
                    : ToGeoJson(json);
            }
            catch
            {
                return coordinatesLegacy ?? string.Empty;
            }
        }

        private static (double lon, double lat) ToWgs84(double x, double y)
        {
            double lon = x * 180.0 / (Math.PI * EarthRadiusMeters);
            double lat = (Math.Atan(Math.Exp(y / EarthRadiusMeters)) * 2.0 - Math.PI / 2.0) * (180.0 / Math.PI);
            return (lon, lat);
        }

        private static string FormatCoord(double x, double y)
        {
            var (lon, lat) = ToWgs84(x, y);
            return lon.ToString("G", CultureInfo.InvariantCulture) + " " + lat.ToString("G", CultureInfo.InvariantCulture);
        }

        private static string ToWkt(JObject json)
        {
            if (json["x"] != null && json["y"] != null)
            {
                // Point
                double x = json["x"]!.Value<double>();
                double y = json["y"]!.Value<double>();
                var (lon, lat) = ToWgs84(x, y);
                return $"POINT({lon.ToString("G", CultureInfo.InvariantCulture)} {lat.ToString("G", CultureInfo.InvariantCulture)})";
            }

            if (json["points"] is JArray pointsArray)
            {
                // Multipoint
                var points = ParsePointArray(pointsArray);
                var sb = new StringBuilder("MULTIPOINT(");
                for (int i = 0; i < points.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append('(');
                    sb.Append(FormatCoord(points[i].x, points[i].y));
                    sb.Append(')');
                }
                sb.Append(')');
                return sb.ToString();
            }

            if (json["paths"] is JArray pathsArray)
            {
                // Polyline / MultiLineString
                var parts = ParsePartsArray(pathsArray);
                if (parts.Count == 1)
                {
                    return "LINESTRING(" + FormatRing(parts[0]) + ")";
                }
                else
                {
                    var sb = new StringBuilder("MULTILINESTRING(");
                    for (int i = 0; i < parts.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('(');
                        sb.Append(FormatRing(parts[i]));
                        sb.Append(')');
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            if (json["rings"] is JArray ringsArray)
            {
                // Polygon / MultiPolygon
                var rings = ParsePartsArray(ringsArray);
                if (rings.Count == 1)
                {
                    return "POLYGON((" + FormatRing(EnsureClosed(rings[0])) + "))";
                }
                else
                {
                    var sb = new StringBuilder("MULTIPOLYGON(");
                    for (int i = 0; i < rings.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("((");
                        sb.Append(FormatRing(EnsureClosed(rings[i])));
                        sb.Append("))");
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        private static string ToGeoJson(JObject json)
        {
            if (json["x"] != null && json["y"] != null)
            {
                double x = json["x"]!.Value<double>();
                double y = json["y"]!.Value<double>();
                var (lon, lat) = ToWgs84(x, y);
                return $"{{\"type\":\"Point\",\"coordinates\":[{lon.ToString("G", CultureInfo.InvariantCulture)},{lat.ToString("G", CultureInfo.InvariantCulture)}]}}";
            }

            if (json["points"] is JArray pointsArray)
            {
                var points = ParsePointArray(pointsArray);
                var sb = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":[");
                for (int i = 0; i < points.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    var (lon, lat) = ToWgs84(points[i].x, points[i].y);
                    sb.Append($"[{lon.ToString("G", CultureInfo.InvariantCulture)},{lat.ToString("G", CultureInfo.InvariantCulture)}]");
                }
                sb.Append("]}");
                return sb.ToString();
            }

            if (json["paths"] is JArray pathsArray)
            {
                var parts = ParsePartsArray(pathsArray);
                if (parts.Count == 1)
                {
                    return "{\"type\":\"LineString\",\"coordinates\":" + FormatGeoJsonRing(parts[0]) + "}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiLineString\",\"coordinates\":[");
                    for (int i = 0; i < parts.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(FormatGeoJsonRing(parts[i]));
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            if (json["rings"] is JArray ringsArray)
            {
                var rings = ParsePartsArray(ringsArray);
                if (rings.Count == 1)
                {
                    return "{\"type\":\"Polygon\",\"coordinates\":[" + FormatGeoJsonRing(EnsureClosed(rings[0])) + "]}";
                }
                else
                {
                    var sb = new StringBuilder("{\"type\":\"MultiPolygon\",\"coordinates\":[");
                    for (int i = 0; i < rings.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('[');
                        sb.Append(FormatGeoJsonRing(EnsureClosed(rings[i])));
                        sb.Append(']');
                    }
                    sb.Append("]}");
                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        private static List<(double x, double y)> ParsePointArray(JArray arr)
        {
            var result = new List<(double x, double y)>();
            foreach (var item in arr)
            {
                if (item is JArray pair && pair.Count >= 2)
                    result.Add((pair[0].Value<double>(), pair[1].Value<double>()));
            }
            return result;
        }

        private static List<List<(double x, double y)>> ParsePartsArray(JArray arr)
        {
            var result = new List<List<(double x, double y)>>();
            foreach (var part in arr)
            {
                if (part is JArray partArray)
                    result.Add(ParsePointArray(partArray));
            }
            return result;
        }

        private static string FormatRing(List<(double x, double y)> points)
        {
            var parts = new string[points.Count];
            for (int i = 0; i < points.Count; i++)
                parts[i] = FormatCoord(points[i].x, points[i].y);
            return string.Join(",", parts);
        }

        private static string FormatGeoJsonRing(List<(double x, double y)> points)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0) sb.Append(',');
                var (lon, lat) = ToWgs84(points[i].x, points[i].y);
                sb.Append($"[{lon.ToString("G", CultureInfo.InvariantCulture)},{lat.ToString("G", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static List<(double x, double y)> EnsureClosed(List<(double x, double y)> ring)
        {
            if (ring.Count < 2) return ring;
            var first = ring[0];
            var last = ring[ring.Count - 1];
            if (Math.Abs(first.x - last.x) > 1e-10 || Math.Abs(first.y - last.y) > 1e-10)
            {
                var closed = new List<(double x, double y)>(ring) { first };
                return closed;
            }
            return ring;
        }
    }
}

using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Models;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestFixture]
    [TestOf(typeof(GeographySerializer))]
    public class GeographySerializerTests
    {
        // Legacy format tests
        [Test]
        public void Legacy_format_returns_coordinates_string()
        {
            var result = GeographySerializer.Serialize(null, "1.5,2.5;3.5,4.5", GeographyExportFormat.Legacy);
            Assert.That(result, Is.EqualTo("1.5,2.5;3.5,4.5"));
        }

        [Test]
        public void Legacy_format_with_null_coordinates_returns_empty()
        {
            var result = GeographySerializer.Serialize(null, null, GeographyExportFormat.Legacy);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        // WKT - Point
        [Test]
        public void Wkt_point_from_esri_json()
        {
            // EPSG:3857 - a known point: lon=0, lat=0 maps to x=0, y=0
            var geometry = "{\"x\":0,\"y\":0,\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, "0,0", GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("POINT("));
            Assert.That(result, Does.Contain("0 0"));
        }

        [Test]
        public void Wkt_point_valid_format()
        {
            // x=1113194.9079, y=0 → lon≈10, lat≈0
            var x = 1113194.9079327357;
            var geometry = $"{{\"x\":{x},\"y\":0}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("POINT("));
            // longitude should be ~10, latitude ~0
        }

        // WKT - Multipoint
        [Test]
        public void Wkt_multipoint_from_esri_json()
        {
            var geometry = "{\"points\":[[0,0],[1113194.9,0]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("MULTIPOINT("));
        }

        // WKT - Polyline (single path)
        [Test]
        public void Wkt_linestring_from_esri_json()
        {
            var geometry = "{\"paths\":[[[0,0],[1113194.9,0],[1113194.9,1113194.9]]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("LINESTRING("));
        }

        // WKT - Polyline (multiple paths)
        [Test]
        public void Wkt_multilinestring_from_esri_json()
        {
            var geometry = "{\"paths\":[[[0,0],[1113194.9,0]],[[0,1113194.9],[1113194.9,1113194.9]]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("MULTILINESTRING("));
        }

        // WKT - Polygon (single ring)
        [Test]
        public void Wkt_polygon_from_esri_json()
        {
            var geometry = "{\"rings\":[[[0,0],[1113194.9,0],[1113194.9,1113194.9],[0,1113194.9],[0,0]]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("POLYGON("));
        }

        // WKT - Polygon (multiple rings = multipolygon)
        [Test]
        public void Wkt_multipolygon_from_esri_json_with_multiple_rings()
        {
            var geometry = "{\"rings\":[[[0,0],[1113194.9,0],[0,1113194.9],[0,0]],[[2226389.8,0],[3339584.7,0],[2226389.8,1113194.9],[2226389.8,0]]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("MULTIPOLYGON("));
        }

        // WKT - invalid geometry falls back to coordinates
        [Test]
        public void Wkt_invalid_geometry_falls_back_to_legacy()
        {
            var result = GeographySerializer.Serialize("invalid-json", "1,2;3,4", GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("1,2;3,4"));
        }

        // GeoJSON - Point
        [Test]
        public void GeoJson_point_from_esri_json()
        {
            var geometry = "{\"x\":0,\"y\":0,\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"Point\""));
            Assert.That(result, Does.Contain("\"coordinates\""));
        }

        // GeoJSON - Multipoint
        [Test]
        public void GeoJson_multipoint_from_esri_json()
        {
            var geometry = "{\"points\":[[0,0],[1113194.9,0]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"MultiPoint\""));
        }

        // GeoJSON - LineString
        [Test]
        public void GeoJson_linestring_from_esri_json()
        {
            var geometry = "{\"paths\":[[[0,0],[1113194.9,0]]],\"spatialReference\":{\"wkid\":102100}}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"LineString\""));
        }

        // GeoJSON - MultiLineString
        [Test]
        public void GeoJson_multilinestring_from_esri_json()
        {
            var geometry = "{\"paths\":[[[0,0],[1113194.9,0]],[[0,1113194.9],[1113194.9,1113194.9]]]}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"MultiLineString\""));
        }

        // GeoJSON - Polygon
        [Test]
        public void GeoJson_polygon_from_esri_json()
        {
            var geometry = "{\"rings\":[[[0,0],[1113194.9,0],[1113194.9,1113194.9],[0,1113194.9],[0,0]]]}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"Polygon\""));
        }

        // GeoJSON - MultiPolygon
        [Test]
        public void GeoJson_multipolygon_from_esri_json_with_multiple_rings()
        {
            var geometry = "{\"rings\":[[[0,0],[1113194.9,0],[0,1113194.9],[0,0]],[[2226389.8,0],[3339584.7,0],[2226389.8,1113194.9],[2226389.8,0]]]}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.GeoJson);
            Assert.That(result, Does.Contain("\"type\":\"MultiPolygon\""));
        }

        // Empty geometry
        [Test]
        public void Wkt_empty_geometry_falls_back_to_coordinates()
        {
            var result = GeographySerializer.Serialize(string.Empty, "1,2", GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("1,2"));
        }

        [Test]
        public void Wkt_null_geometry_falls_back_to_coordinates()
        {
            var result = GeographySerializer.Serialize(null, "5,10", GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("5,10"));
        }

        // Ring closure
        [Test]
        public void Wkt_polygon_closes_ring_if_not_closed()
        {
            // Ring without closing point
            var geometry = "{\"rings\":[[[0,0],[1113194.9,0],[1113194.9,1113194.9],[0,1113194.9]]]}";
            var result = GeographySerializer.Serialize(geometry, null, GeographyExportFormat.Wkt);
            Assert.That(result, Does.StartWith("POLYGON("));
            // Should have ring closed: start=end point
            var inner = result.Substring("POLYGON((".Length, result.Length - "POLYGON((".Length - "))".Length);
            var points = inner.Split(',');
            Assert.That(points[0], Is.EqualTo(points[points.Length - 1]));
        }
    }
}

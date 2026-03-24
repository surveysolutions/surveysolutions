using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;
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
            var area = new Area { Geometry = "{\"x\":10.5,\"y\":48.2}", Coordinates = "1.5,2.5;3.5,4.5" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Legacy);
            Assert.That(result, Is.EqualTo("1.5,2.5;3.5,4.5"));
        }

        [Test]
        public void Legacy_format_with_null_area_returns_empty()
        {
            var result = GeographySerializer.Serialize(null, GeographyExportFormat.Legacy);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Null_area_returns_empty_for_wkt()
        {
            var result = GeographySerializer.Serialize(null, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        // WKT - Point (WGS84 coordinates directly)
        [Test]
        public void Wkt_point_from_esri_json()
        {
            var area = new Area { Geometry = "{\"x\":10.5,\"y\":48.2}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("POINT(10.5 48.2)"));
        }

        // WKT - Multipoint
        [Test]
        public void Wkt_multipoint_from_esri_json()
        {
            var area = new Area { Geometry = "{\"points\":[[10.5,48.2],[11.0,49.0]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("MULTIPOINT((10.5 48.2),(11 49))"));
        }

        // WKT - Polyline (single path)
        [Test]
        public void Wkt_linestring_from_esri_json()
        {
            var area = new Area { Geometry = "{\"paths\":[[[10.0,48.0],[11.0,49.0],[12.0,50.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("LINESTRING(10 48,11 49,12 50)"));
        }

        // WKT - Polyline (multiple paths)
        [Test]
        public void Wkt_multilinestring_from_esri_json()
        {
            var area = new Area { Geometry = "{\"paths\":[[[10.0,48.0],[11.0,49.0]],[[12.0,50.0],[13.0,51.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("MULTILINESTRING((10 48,11 49),(12 50,13 51))"));
        }

        // WKT - Polygon (single ring)
        [Test]
        public void Wkt_polygon_from_esri_json()
        {
            var area = new Area { Geometry = "{\"rings\":[[[10.0,48.0],[11.0,48.0],[11.0,49.0],[10.0,48.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("POLYGON((10 48,11 48,11 49,10 48))"));
        }

        // WKT - Polygon (multiple rings = multipolygon)
        [Test]
        public void Wkt_multipolygon_from_esri_json_with_multiple_rings()
        {
            var area = new Area { Geometry = "{\"rings\":[[[10.0,48.0],[11.0,48.0],[10.0,49.0],[10.0,48.0]],[[20.0,48.0],[21.0,48.0],[20.0,49.0],[20.0,48.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("MULTIPOLYGON(((10 48,11 48,10 49,10 48)),((20 48,21 48,20 49,20 48)))"));
        }

        // WKT - invalid geometry falls back to coordinates
        [Test]
        public void Wkt_invalid_geometry_falls_back_to_legacy()
        {
            var area = new Area { Geometry = "invalid-json", Coordinates = "1,2;3,4" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("1,2;3,4"));
        }

        // GeoJSON - Point
        [Test]
        public void GeoJson_point_from_esri_json()
        {
            var area = new Area { Geometry = "{\"x\":10.5,\"y\":48.2}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"Point\",\"coordinates\":[10.5,48.2]}"));
        }

        // GeoJSON - Multipoint
        [Test]
        public void GeoJson_multipoint_from_esri_json()
        {
            var area = new Area { Geometry = "{\"points\":[[10.5,48.2],[11.0,49.0]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"MultiPoint\",\"coordinates\":[[10.5,48.2],[11,49]]}"));
        }

        // GeoJSON - LineString
        [Test]
        public void GeoJson_linestring_from_esri_json()
        {
            var area = new Area { Geometry = "{\"paths\":[[[10.0,48.0],[11.0,49.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"LineString\",\"coordinates\":[[10,48],[11,49]]}"));
        }

        // GeoJSON - MultiLineString
        [Test]
        public void GeoJson_multilinestring_from_esri_json()
        {
            var area = new Area { Geometry = "{\"paths\":[[[10.0,48.0],[11.0,49.0]],[[12.0,50.0],[13.0,51.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"MultiLineString\",\"coordinates\":[[[10,48],[11,49]],[[12,50],[13,51]]]}"));
        }

        // GeoJSON - Polygon
        [Test]
        public void GeoJson_polygon_from_esri_json()
        {
            var area = new Area { Geometry = "{\"rings\":[[[10.0,48.0],[11.0,48.0],[11.0,49.0],[10.0,48.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[10,48],[11,48],[11,49],[10,48]]]}"));
        }

        // GeoJSON - MultiPolygon
        [Test]
        public void GeoJson_multipolygon_from_esri_json_with_multiple_rings()
        {
            var area = new Area { Geometry = "{\"rings\":[[[10.0,48.0],[11.0,48.0],[10.0,49.0],[10.0,48.0]],[[20.0,48.0],[21.0,48.0],[20.0,49.0],[20.0,48.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson);
            Assert.That(result, Is.EqualTo("{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,48],[11,48],[10,49],[10,48]]],[[[20,48],[21,48],[20,49],[20,48]]]]}"));
        }

        // Empty geometry
        [Test]
        public void Wkt_empty_geometry_falls_back_to_coordinates()
        {
            var area = new Area { Geometry = string.Empty, Coordinates = "1,2" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("1,2"));
        }

        [Test]
        public void Wkt_null_geometry_falls_back_to_coordinates()
        {
            var area = new Area { Geometry = null!, Coordinates = "5,10" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("5,10"));
        }

        // Ring closure
        [Test]
        public void Wkt_polygon_closes_ring_if_not_closed()
        {
            // Ring without closing point
            var area = new Area { Geometry = "{\"rings\":[[[10.0,48.0],[11.0,48.0],[11.0,49.0],[10.0,49.0]]]}" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("POLYGON((10 48,11 48,11 49,10 49,10 48))"));
        }
    }
}

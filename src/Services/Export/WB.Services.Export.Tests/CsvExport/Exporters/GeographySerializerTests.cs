using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    /// <summary>
    /// Tests for GeographySerializer.
    /// Coordinates in area.Coordinates are WGS84 (lon,lat) semicolon-separated.
    /// area.Geometry (Esri JSON) is used only for geometry type and structure.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(GeographySerializer))]
    public class GeographySerializerTests
    {
        // ── Legacy ──────────────────────────────────────────────────────────────

        [Test]
        public void Legacy_format_returns_coordinates_string()
        {
            var area = new Area { Geometry = "{\"x\":0,\"y\":0}", Coordinates = "1.5,2.5;3.5,4.5" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Legacy);
            Assert.That(result, Is.EqualTo("1.5,2.5;3.5,4.5"));
        }

        [Test]
        public void Legacy_null_area_returns_empty()
        {
            Assert.That(GeographySerializer.Serialize(null, GeographyExportFormat.Legacy), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Wkt_null_area_returns_empty()
        {
            Assert.That(GeographySerializer.Serialize(null, GeographyExportFormat.Wkt), Is.EqualTo(string.Empty));
        }

        // ── Empty/fallback ──────────────────────────────────────────────────────

        [Test]
        public void Wkt_empty_coordinates_returns_empty()
        {
            var area = new Area { Geometry = "{\"x\":10.5,\"y\":48.2}", Coordinates = string.Empty };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Wkt_null_coordinates_returns_empty()
        {
            var area = new Area { Geometry = "{\"x\":10.5,\"y\":48.2}", Coordinates = null! };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Wkt_invalid_geometry_json_falls_back_to_legacy_coordinates()
        {
            var area = new Area { Geometry = "not-json", Coordinates = "10.5,48.2" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("10.5,48.2"));
        }

        [Test]
        public void Wkt_empty_geometry_json_falls_back_to_legacy_coordinates()
        {
            var area = new Area { Geometry = string.Empty, Coordinates = "10.5,48.2" };
            var result = GeographySerializer.Serialize(area, GeographyExportFormat.Wkt);
            Assert.That(result, Is.EqualTo("10.5,48.2"));
        }

        // ── WKT – Point ─────────────────────────────────────────────────────────
        // Geometry JSON: {"x": ..., "y": ...}
        // Coordinates: "lon,lat"

        [Test]
        public void Wkt_point()
        {
            var area = new Area
            {
                Geometry = "{\"x\":0,\"y\":0}",
                Coordinates = "10.5,48.2"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt), Is.EqualTo("POINT(10.5 48.2)"));
        }

        // ── WKT – Multipoint ────────────────────────────────────────────────────
        // Geometry JSON: {"points":[[..],[..]]}
        // Coordinates: "lon1,lat1;lon2,lat2"

        [Test]
        public void Wkt_multipoint()
        {
            var area = new Area
            {
                Geometry = "{\"points\":[[0,0],[0,0]]}",
                Coordinates = "10.5,48.2;11,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("MULTIPOINT((10.5 48.2),(11 49))"));
        }

        // ── WKT – LineString ────────────────────────────────────────────────────
        // Geometry JSON: {"paths":[[[..],[..],[..]]]}  (single path, 3 points)
        // Coordinates: "lon1,lat1;lon2,lat2;lon3,lat3"

        [Test]
        public void Wkt_linestring_single_path()
        {
            var area = new Area
            {
                Geometry = "{\"paths\":[[[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,49;12,50"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("LINESTRING(10 48,11 49,12 50)"));
        }

        // ── WKT – MultiLineString ───────────────────────────────────────────────
        // Geometry JSON: {"paths":[[[..],[..]],[[..],[..]]]}  (2 paths, 2 pts each)
        // Coordinates: "lon1,lat1;lon2,lat2;lon3,lat3;lon4,lat4"

        [Test]
        public void Wkt_multilinestring_two_paths()
        {
            var area = new Area
            {
                Geometry = "{\"paths\":[[[0,0],[0,0]],[[0,0],[0,0]]]}",
                Coordinates = "10,48;11,49;12,50;13,51"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("MULTILINESTRING((10 48,11 49),(12 50,13 51))"));
        }

        // ── WKT – Polygon ───────────────────────────────────────────────────────
        // Geometry JSON: {"rings":[[[..],[..],[..],[..]]]}  (single ring, 4 pts, already closed)
        // Coordinates: "lon1,lat1;lon2,lat2;lon3,lat3;lon4,lat4"

        [Test]
        public void Wkt_polygon_single_ring_already_closed()
        {
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,48;11,49;10,48"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("POLYGON((10 48,11 48,11 49,10 48))"));
        }

        [Test]
        public void Wkt_polygon_single_ring_unclosed_gets_closed()
        {
            // 3 distinct points without closing → EnsureClosed should add first point at end
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,48;11,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("POLYGON((10 48,11 48,11 49,10 48))"));
        }

        // ── WKT – MultiPolygon ─────────────────────────────────────────────────
        // Geometry JSON: {"rings":[[[..],[..],[..]],[[..],[..],[..]]]}  (2 rings, 3+3 pts)
        // Coordinates: flat 6 pts

        [Test]
        public void Wkt_multipolygon_two_rings()
        {
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0]],[[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,48;10,49;20,48;21,48;20,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("MULTIPOLYGON(((10 48,11 48,10 49,10 48)),((20 48,21 48,20 49,20 48)))"));
        }

        // ── WKT – coordinate count mismatch ────────────────────────────────────

        [Test]
        public void Wkt_coordinate_count_mismatch_falls_back_to_legacy()
        {
            // Geometry says 3-point ring but coordinates has only 2 points
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,49"  // only 2, but ring expects 3
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.Wkt),
                Is.EqualTo("10,48;11,49"));
        }

        // ── GeoJSON – Point ─────────────────────────────────────────────────────

        [Test]
        public void GeoJson_point()
        {
            var area = new Area
            {
                Geometry = "{\"x\":0,\"y\":0}",
                Coordinates = "10.5,48.2"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"Point\",\"coordinates\":[10.5,48.2]}"));
        }

        // ── GeoJSON – MultiPoint ────────────────────────────────────────────────

        [Test]
        public void GeoJson_multipoint()
        {
            var area = new Area
            {
                Geometry = "{\"points\":[[0,0],[0,0]]}",
                Coordinates = "10.5,48.2;11,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"MultiPoint\",\"coordinates\":[[10.5,48.2],[11,49]]}"));
        }

        // ── GeoJSON – LineString ────────────────────────────────────────────────

        [Test]
        public void GeoJson_linestring_single_path()
        {
            var area = new Area
            {
                Geometry = "{\"paths\":[[[0,0],[0,0]]]}",
                Coordinates = "10,48;11,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"LineString\",\"coordinates\":[[10,48],[11,49]]}"));
        }

        // ── GeoJSON – MultiLineString ───────────────────────────────────────────

        [Test]
        public void GeoJson_multilinestring_two_paths()
        {
            var area = new Area
            {
                Geometry = "{\"paths\":[[[0,0],[0,0]],[[0,0],[0,0]]]}",
                Coordinates = "10,48;11,49;12,50;13,51"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"MultiLineString\",\"coordinates\":[[[10,48],[11,49]],[[12,50],[13,51]]]}"));
        }

        // ── GeoJSON – Polygon ───────────────────────────────────────────────────

        [Test]
        public void GeoJson_polygon_single_ring()
        {
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,48;11,49;10,48"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[10,48],[11,48],[11,49],[10,48]]]}"));
        }

        // ── GeoJSON – MultiPolygon ─────────────────────────────────────────────

        [Test]
        public void GeoJson_multipolygon_two_rings()
        {
            var area = new Area
            {
                Geometry = "{\"rings\":[[[0,0],[0,0],[0,0]],[[0,0],[0,0],[0,0]]]}",
                Coordinates = "10,48;11,48;10,49;20,48;21,48;20,49"
            };
            Assert.That(GeographySerializer.Serialize(area, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,48],[11,48],[10,49],[10,48]]],[[[20,48],[21,48],[20,49],[20,48]]]]}"));
        }
    }
}

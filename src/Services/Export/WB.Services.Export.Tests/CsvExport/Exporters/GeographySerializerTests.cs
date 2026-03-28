using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    /// <summary>
    /// Tests for GeographySerializer.
    /// Coordinates in area.Coordinates are WGS84 (lon,lat) semicolon-separated,
    /// matching the output of GeometryHelper.GetProjectedCoordinates on the mobile side.
    /// GeometryType comes from the questionnaire question definition.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(GeographySerializer))]
    public class GeographySerializerTests
    {
        private IGeographySerializer serializer = null!;

        [SetUp]
        public void SetUp()
        {
            serializer = new GeographySerializer();
        }

        // ── Null / empty ────────────────────────────────────────────────────────

        [Test]
        public void Null_area_returns_empty_for_legacy()
            => Assert.That(serializer.Serialize(null, GeometryType.Point, GeographyExportFormat.Legacy), Is.EqualTo(string.Empty));

        [Test]
        public void Null_area_returns_empty_for_wkt()
            => Assert.That(serializer.Serialize(null, GeometryType.Point, GeographyExportFormat.Wkt), Is.EqualTo(string.Empty));

        [Test]
        public void Null_geometry_type_falls_back_to_legacy_coordinates()
        {
            var area = new Area { Coordinates = "10.5,48.2" };
            Assert.That(serializer.Serialize(area, null, GeographyExportFormat.Wkt), Is.EqualTo("10.5,48.2"));
        }

        [Test]
        public void Empty_coordinates_returns_empty()
        {
            var area = new Area { Coordinates = string.Empty };
            Assert.That(serializer.Serialize(area, GeometryType.Point, GeographyExportFormat.Wkt), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Invalid_coordinates_falls_back_to_legacy_coordinates()
        {
            var area = new Area { Coordinates = "not-valid" };
            Assert.That(serializer.Serialize(area, GeometryType.Point, GeographyExportFormat.Wkt), Is.EqualTo("not-valid"));
        }

        // ── Legacy ──────────────────────────────────────────────────────────────

        [Test]
        public void Legacy_format_returns_coordinates_string()
        {
            var area = new Area { Coordinates = "1.5,2.5;3.5,4.5" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.Legacy), Is.EqualTo("1.5,2.5;3.5,4.5"));
        }

        // ── WKT – Point ─────────────────────────────────────────────────────────

        [Test]
        public void Wkt_point()
        {
            var area = new Area { Coordinates = "10.5,48.2" };
            Assert.That(serializer.Serialize(area, GeometryType.Point, GeographyExportFormat.Wkt),
                Is.EqualTo("POINT(10.5 48.2)"));
        }

        // ── WKT – Multipoint ────────────────────────────────────────────────────

        [Test]
        public void Wkt_multipoint()
        {
            var area = new Area { Coordinates = "10.5,48.2;11,49" };
            Assert.That(serializer.Serialize(area, GeometryType.Multipoint, GeographyExportFormat.Wkt),
                Is.EqualTo("MULTIPOINT((10.5 48.2),(11 49))"));
        }

        // ── WKT – LineString ────────────────────────────────────────────────────

        [Test]
        public void Wkt_linestring()
        {
            var area = new Area { Coordinates = "10,48;11,49;12,50" };
            Assert.That(serializer.Serialize(area, GeometryType.Polyline, GeographyExportFormat.Wkt),
                Is.EqualTo("LINESTRING(10 48,11 49,12 50)"));
        }

        [Test]
        public void Wkt_linestring_with_fewer_than_2_coords_falls_back_to_legacy()
        {
            var area = new Area { Coordinates = "10,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polyline, GeographyExportFormat.Wkt),
                Is.EqualTo("10,48"));
        }

        // ── WKT – Polygon ───────────────────────────────────────────────────────

        [Test]
        public void Wkt_polygon_already_closed()
        {
            var area = new Area { Coordinates = "10,48;11,48;11,49;10,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.Wkt),
                Is.EqualTo("POLYGON((10 48,11 48,11 49,10 48))"));
        }

        [Test]
        public void Wkt_polygon_unclosed_gets_closed()
        {
            var area = new Area { Coordinates = "10,48;11,48;11,49" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.Wkt),
                Is.EqualTo("POLYGON((10 48,11 48,11 49,10 48))"));
        }

        [Test]
        public void Wkt_polygon_with_fewer_than_3_coords_falls_back_to_legacy()
        {
            var area = new Area { Coordinates = "10,48;11,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.Wkt),
                Is.EqualTo("10,48;11,48"));
        }

        // ── GeoJSON – Point ─────────────────────────────────────────────────────

        [Test]
        public void GeoJson_point()
        {
            var area = new Area { Coordinates = "10.5,48.2" };
            Assert.That(serializer.Serialize(area, GeometryType.Point, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"Point\",\"coordinates\":[10.5,48.2]}"));
        }

        // ── GeoJSON – MultiPoint ────────────────────────────────────────────────

        [Test]
        public void GeoJson_multipoint()
        {
            var area = new Area { Coordinates = "10.5,48.2;11,49" };
            Assert.That(serializer.Serialize(area, GeometryType.Multipoint, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"MultiPoint\",\"coordinates\":[[10.5,48.2],[11,49]]}"));
        }

        // ── GeoJSON – LineString ────────────────────────────────────────────────

        [Test]
        public void GeoJson_linestring()
        {
            var area = new Area { Coordinates = "10,48;11,49" };
            Assert.That(serializer.Serialize(area, GeometryType.Polyline, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"LineString\",\"coordinates\":[[10,48],[11,49]]}"));
        }

        [Test]
        public void GeoJson_linestring_with_fewer_than_2_coords_falls_back_to_legacy()
        {
            var area = new Area { Coordinates = "10,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polyline, GeographyExportFormat.GeoJson),
                Is.EqualTo("10,48"));
        }

        // ── GeoJSON – Polygon ───────────────────────────────────────────────────

        [Test]
        public void GeoJson_polygon_already_closed()
        {
            var area = new Area { Coordinates = "10,48;11,48;11,49;10,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[10,48],[11,48],[11,49],[10,48]]]}"));
        }

        [Test]
        public void GeoJson_polygon_unclosed_gets_closed()
        {
            var area = new Area { Coordinates = "10,48;11,48;11,49" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.GeoJson),
                Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[10,48],[11,48],[11,49],[10,48]]]}"));
        }

        [Test]
        public void GeoJson_polygon_with_fewer_than_3_coords_falls_back_to_legacy()
        {
            var area = new Area { Coordinates = "10,48;11,48" };
            Assert.That(serializer.Serialize(area, GeometryType.Polygon, GeographyExportFormat.GeoJson),
                Is.EqualTo("10,48;11,48"));
        }
    }
}

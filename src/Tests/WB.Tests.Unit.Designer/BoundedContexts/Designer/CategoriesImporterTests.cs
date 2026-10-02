#nullable enable
using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAssertions;
using NUnit.Framework;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(CategoriesImporter))]
    public class CategoriesImporterTests
    {
        private static Stream Workbook(string[] headers, params object[][] rows)
        {
            // importer reads A1:D1 into a dictionary, so pad to four unique header names
            headers = headers.Concat(Enumerable.Range(headers.Length, 4 - headers.Length).Select(i => "extra" + i)).ToArray();
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Categories");
            for (var c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            for (var r = 0; r < rows.Length; r++)
                for (var c = 0; c < rows[r].Length; c++)
                    ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(rows[r][c]);
            var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        [Test]
        public void when_value_title_and_parent_headers_should_extract_items()
        {
            var stream = Workbook(new[] { "value", "title", "parentvalue", "attachmentname" },
                new object[] { 1, "One", "" },
                new object[] { 2, "Two", 1 });

            var items = new CategoriesImporter().ExtractCategoriesFromExcelFile(stream);

            items.Should().HaveCount(2);
            items[0].Id.Should().Be(1);
            items[0].Text.Should().Be("One");
            items[0].ParentId.Should().BeNull();
            items[1].ParentId.Should().Be(1);
        }

        [Test]
        public void when_only_value_and_title_headers_should_extract_items_without_parent_and_attachment()
        {
            var stream = Workbook(new[] { "value", "title" }, new object[] { 1, "One" });

            var item = new CategoriesImporter().ExtractCategoriesFromExcelFile(stream).Single();

            item.Id.Should().Be(1);
            item.ParentId.Should().BeNull();
            item.AttachmentName.Should().BeNull();
        }

        [Test]
        public void when_headers_have_different_case_should_still_match()
        {
            var stream = Workbook(new[] { "Value", "Title" }, new object[] { 4, "Four" });

            new CategoriesImporter().ExtractCategoriesFromExcelFile(stream).Single().Text.Should().Be("Four");
        }

        [Test]
        public void when_alternative_id_and_text_headers_should_extract_items()
        {
            var stream = Workbook(new[] { "id", "text", "parentid", "attachmentname" }, new object[] { 7, "Seven" });

            var items = new CategoriesImporter().ExtractCategoriesFromExcelFile(stream);

            items.Single().Id.Should().Be(7);
            items.Single().Text.Should().Be("Seven");
        }

        [Test]
        public void when_row_has_empty_id_should_skip_it()
        {
            var stream = Workbook(new[] { "value", "title", "parentvalue", "attachmentname" },
                new object[] { "", "Skipped" },
                new object[] { 3, "Three" });

            var items = new CategoriesImporter().ExtractCategoriesFromExcelFile(stream);

            items.Should().ContainSingle().Which.Id.Should().Be(3);
        }

        [Test]
        public void when_id_header_is_missing_should_throw()
        {
            var stream = Workbook(new[] { "title" }, new object[] { "x" });

            var ex = Assert.Throws<InvalidOperationException>(() => new CategoriesImporter().ExtractCategoriesFromExcelFile(stream));

            ex!.Message.Should().Contain("value");
        }

        [Test]
        public void when_title_header_is_missing_should_throw()
        {
            var stream = Workbook(new[] { "value" }, new object[] { 1 });

            var ex = Assert.Throws<InvalidOperationException>(() => new CategoriesImporter().ExtractCategoriesFromExcelFile(stream));

            ex!.Message.Should().Contain("title");
        }

        [Test]
        public void when_only_header_row_should_throw()
        {
            var stream = Workbook(new[] { "value", "title" });

            Assert.Throws<InvalidOperationException>(() => new CategoriesImporter().ExtractCategoriesFromExcelFile(stream));
        }
    }
}



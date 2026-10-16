using System.IO;
using System.Linq;
using ClosedXML.Excel;
using NUnit.Framework;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Tests.Unit.Infrastructure.Native
{
    [TestOf(typeof(CategoriesImporter))]
    public class CategoriesImporterTests
    {
        [Test]
        public void when_file_has_no_parent_column_should_extract_categories_without_parent()
        {
            var file = CreateExcel(
                new[] { "value", "title", "attachmentname" },
                new[] { "1", "first", "image.png" },
                new[] { "2", "second", "" });

            var categories = new CategoriesImporter().ExtractCategoriesFromExcelFile(file);

            Assert.That(categories.Select(x => x.Id), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(categories.Select(x => x.Text), Is.EqualTo(new[] { "first", "second" }));
            Assert.That(categories.Select(x => x.ParentId), Is.EqualTo(new int?[] { null, null }));
            Assert.That(categories.Select(x => x.AttachmentName), Is.EqualTo(new[] { "image.png", string.Empty }));
        }

        [Test]
        public void when_file_has_no_attachment_column_should_extract_categories_without_attachment()
        {
            var file = CreateExcel(
                new[] { "value", "title", "parentvalue" },
                new[] { "1", "first", "" },
                new[] { "2", "second", "1" });

            var categories = new CategoriesImporter().ExtractCategoriesFromExcelFile(file);

            Assert.That(categories.Select(x => x.ParentId), Is.EqualTo(new int?[] { null, 1 }));
            Assert.That(categories.Select(x => x.AttachmentName), Is.EqualTo(new[] { string.Empty, string.Empty }));
        }

        [Test]
        public void when_file_has_all_columns_should_extract_parent_and_attachment()
        {
            var file = CreateExcel(
                new[] { "value", "title", "parentvalue", "attachmentname" },
                new[] { "1", "first", "", "image.png" },
                new[] { "2", "second", "1", "" });

            var categories = new CategoriesImporter().ExtractCategoriesFromExcelFile(file);

            Assert.That(categories.Select(x => x.ParentId), Is.EqualTo(new int?[] { null, 1 }));
            Assert.That(categories.Select(x => x.AttachmentName), Is.EqualTo(new[] { "image.png", string.Empty }));
        }

        private static Stream CreateExcel(params string[][] rows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Categories");

            for (var row = 0; row < rows.Length; row++)
                for (var column = 0; column < rows[row].Length; column++)
                    worksheet.Cell(row + 1, column + 1).SetValue(rows[row][column]);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}

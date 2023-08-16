using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using SixLabors.Fonts;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class CategoriesImporter : ICategoriesImporter
    {
        public List<CategoriesItem> ExtractCategoriesFromExcelFile(Stream xmlFile)
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            var categories = new List<CategoriesItem>();
            using XLWorkbook package = new XLWorkbook(xmlFile, loadOptions);
            var worksheet = package.Worksheets.First();
            var headers = GetHeaders(worksheet);

            var rowsCount = worksheet.LastRowUsed().RowNumber();

            if (headers.IdIndex == null)
                throw new InvalidOperationException("Header (value) was not found.");

            if (headers.TextIndex == null)
                throw new InvalidOperationException("Header (title) was not found.");

            if (rowsCount == 1)
                throw new InvalidOperationException("Categories were not found.");

            for (int rowNumber = 2; rowNumber <= rowsCount; rowNumber++)
            {
                var row = GetRowValues(worksheet, headers, rowNumber);

                if (row == null) continue;

                else categories.Add(row);
            }

            return categories;
        }

        private CategoriesHeaderMap GetHeaders(IXLWorksheet worksheet)
        {
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(worksheet.Cell("A1").GetString(), "A"),
                new Tuple<string, string>(worksheet.Cell("B1").GetString(), "B"),
                new Tuple<string, string>(worksheet.Cell("C1").GetString(), "C"),
                new Tuple<string, string>(worksheet.Cell("D1").GetString(), "D"),
            }.Where(kv => kv.Item1 != null).ToDictionary(k => k.Item1.Trim(), v => v.Item2);

            return new CategoriesHeaderMap()
            {
                IdIndex = headers.GetOrNull("value") ?? headers.GetOrNull("id"),
                ParentIdIndex = headers.GetOrNull("parentvalue") ?? headers.GetOrNull("parentid"),
                TextIndex = headers.GetOrNull("title") ?? headers.GetOrNull("text"),
                AttachmentNameIndex = headers.GetOrNull("attachmentname"),
            };
        }

        private CategoriesItem GetRowValues(IXLWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber)
        {
            var id = worksheet.Cell($"{headers.IdIndex}{rowNumber}").GetString();
            var parentId = worksheet.Cell($"{headers.ParentIdIndex}{rowNumber}").GetString();

            if (string.IsNullOrEmpty(id))
                return null;

            return new CategoriesItem()
            {
                Id = int.Parse(id),
                Text = worksheet.Cell($"{headers.TextIndex}{rowNumber}").GetString(),
                ParentId = string.IsNullOrEmpty(parentId) ? (int?)null : int.Parse(parentId),
                AttachmentName = worksheet.Cell($"{headers.AttachmentNameIndex}{rowNumber}").GetString(),
            };
        }

        internal class CategoriesHeaderMap
        {
            public string IdIndex { get; set; }
            public string ParentIdIndex { get; set; }
            public string TextIndex { get; set; }
            public string AttachmentNameIndex { get; set; }
        }
    }
}

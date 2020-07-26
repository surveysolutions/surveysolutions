using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class CategoriesExportService : ICategoriesExportService
    {
        public byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items)
        {
            using (var excelPackage = new XLWorkbook())
            {
                var worksheet = excelPackage.Worksheets.Add("Categories");

                worksheet.Cell("A1").Value = "id";
                worksheet.Cell("B1").Value = "text";
                worksheet.Cell("C1").Value = "parentid";

                void FormatCell(string address)
                {
                    var cell = worksheet.Cell(address);
                    cell.Style.Font.Bold = true;
                }

                FormatCell("A1");
                FormatCell("B1");
                FormatCell("C1");

                int currentRowNumber = 1;

                foreach (var row in items)
                {
                    currentRowNumber++;

                    worksheet.Cell($"A{currentRowNumber}").Value = row.Id;
                    worksheet.Cell($"A{currentRowNumber}").Style.Alignment.WrapText = true;
                    worksheet.Cell($"B{currentRowNumber}").Value = row.Text;
                    worksheet.Cell($"B{currentRowNumber}").Style.Alignment.WrapText = true;
                    worksheet.Cell($"C{currentRowNumber}").Value = row.ParentId;
                    worksheet.Cell($"C{currentRowNumber}").Style.Alignment.WrapText = true;
                }

                worksheet.Column(3).AdjustToContents();
                worksheet.Protection.AllowElement(XLSheetProtectionElements.FormatColumns);

                using var stream = new MemoryStream();

                excelPackage.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}

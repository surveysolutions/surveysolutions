using System.Collections.Generic;
using OfficeOpenXml;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class CategoriesExportService : ICategoriesExportService
    {
        public byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Categories");

                worksheet.Cells["A1"].Value = "id";
                worksheet.Cells["B1"].Value = "text";
                worksheet.Cells["C1"].Value = "parentid";

                void FormatCell(string address)
                {
                    var cell = worksheet.Cells[address];
                    cell.Style.Font.Bold = true;
                }

                FormatCell("A1");
                FormatCell("B1");
                FormatCell("C1");

                int currentRowNumber = 1;

                foreach (var row in items)
                {
                    currentRowNumber++;

                    worksheet.Cells[$"A{currentRowNumber}"].Value = row.Id;
                    worksheet.Cells[$"A{currentRowNumber}"].Style.WrapText = true;
                    worksheet.Cells[$"B{currentRowNumber}"].Value = row.Text;
                    worksheet.Cells[$"B{currentRowNumber}"].Style.WrapText = true;
                    worksheet.Cells[$"C{currentRowNumber}"].Value = row.ParentId;
                    worksheet.Cells[$"C{currentRowNumber}"].Style.WrapText = true;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Column(3).AutoFit();
                worksheet.Protection.AllowFormatColumns = true;

                return excelPackage.GetAsByteArray();
            }
        }
    }
}

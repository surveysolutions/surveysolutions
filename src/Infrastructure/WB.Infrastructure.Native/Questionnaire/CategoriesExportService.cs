using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.ReusableCategories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class CategoriesExportService : ICategoriesExportService
    {
        public byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items, bool isCascading, bool hqImport)
        {
            using (var excelPackage = new XLWorkbook())
            {
                var worksheet = excelPackage.Worksheets.Add("Categories");

                worksheet.Cell("A1").Value = hqImport ? "id" : "value";
                FormatCell("A1");
                worksheet.Cell("B1").Value = hqImport ? "text" : "title";
                FormatCell("B1");

                if (hqImport || isCascading)
                {
                    worksheet.Cell("C1").Value = hqImport ? "parentid" : "parentvalue";
                    FormatCell("C1");
                    worksheet.Cells("D1").Value = "attachmentname";
                    FormatCell("D1");
                }
                else
                {
                    worksheet.Cells("C1").Value = "attachmentname";
                    FormatCell("C1");
                }

                void FormatCell(string address)
                {
                    var cell = worksheet.Cell(address);
                    cell.Style.Font.Bold = true;
                }

                int currentRowNumber = 1;

                foreach (var row in items)
                {
                    currentRowNumber++;

                    worksheet.Cell($"A{currentRowNumber}").Value = row.Id;
                    worksheet.Cell($"A{currentRowNumber}").Style.Alignment.WrapText = true;
                    worksheet.Cell($"B{currentRowNumber}").Value = row.Text;
                    worksheet.Cell($"B{currentRowNumber}").Style.Alignment.WrapText = true;

                    if (isCascading)
                    {
                        worksheet.Cell($"C{currentRowNumber}").Value = row.ParentId;
                        worksheet.Cell($"C{currentRowNumber}").Style.Alignment.WrapText = true;
                        worksheet.Cell($"D{currentRowNumber}").Value = row.AttachmentName;
                        worksheet.Cell($"D{currentRowNumber}").Style.Alignment.WrapText = true;
                        worksheet.Column(3).AdjustToContents();
                        worksheet.Column(4).AdjustToContents();
                    }
                    else
                    {
                        worksheet.Cell($"C{currentRowNumber}").Value = row.AttachmentName;
                        worksheet.Cell($"C{currentRowNumber}").Style.Alignment.WrapText = true;
                        worksheet.Column(3).AdjustToContents();
                    }
                }

                worksheet.Protection.AllowElement(XLSheetProtectionElements.FormatColumns);

                using var stream = new MemoryStream();

                excelPackage.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}

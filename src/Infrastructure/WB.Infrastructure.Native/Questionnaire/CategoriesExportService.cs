using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using SixLabors.Fonts;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.ReusableCategories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class CategoriesExportService : ICategoriesExportService
    {
        public byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items, bool isCascading, bool hqImport)
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            using (var excelPackage = new XLWorkbook(loadOptions))
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

                    worksheet.Cell(currentRowNumber, 1).Value = row.Id;
                    worksheet.Cell(currentRowNumber, 2).Value = row.Text;

                    if (isCascading)
                    {
                        worksheet.Cell(currentRowNumber, 3).Value = row.ParentId;
                        worksheet.Cell(currentRowNumber, 4).Value = row.AttachmentName;
                    }
                    else
                    {
                        worksheet.Cell(currentRowNumber, 3).Value = row.AttachmentName;
                    }
                }

                worksheet.Cells().Style.Alignment.WrapText = true;
                worksheet.Protection.AllowElement(XLSheetProtectionElements.FormatColumns);
                using var stream = new MemoryStream();

                excelPackage.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}

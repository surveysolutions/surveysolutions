using System.IO;
using ClosedXML.Excel;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class ExcelExportFile : ExportFile
    {
        public override byte[] GetFileBytes(ReportView report)
        {
            var headers = report.Headers;
            var data = report.Data;

            using (XLWorkbook excelPackage = new XLWorkbook())
            {
                var sheetName = report.Name == null
                        ? "Data"
                        : (report.Name.Length > 31 ? report.Name?.Substring(0, 31) : report.Name );

                var worksheet = excelPackage.Worksheets.Add(sheetName);
                var rowIndex = 1;

                // setting headers
                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    var cell = worksheet.Cell(rowIndex, columnIndex + 1);
                    cell.Value = headers[columnIndex];
                    cell.Style.Font.Bold = true;
                }

                rowIndex++;

                // setting table totals if exists
                if (report.Totals != null)
                {
                    for (int columnIndex = 0; columnIndex < report.Totals.Length; columnIndex++)
                    {
                        var cell = worksheet.Cell(rowIndex, columnIndex + 1);
                        cell.Style.Font.Bold = true;
                        var value = report.Totals[columnIndex];

                        SetCellValue(value, cell);
                    }

                    rowIndex++;
                }

                // setting table data
                for (int dataIndex = 0; dataIndex < data.Length; dataIndex++)
                {
                    var rowData = data[dataIndex];

                    for (int columnIndex = 0; columnIndex < rowData.Length; columnIndex++)
                    {
                        var cell = worksheet.Cell(rowIndex, columnIndex + 1);
                        var value = rowData[columnIndex];


                        SetCellValue(value, cell);
                    }

                    rowIndex++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                excelPackage.SaveAs(stream);
                
                return stream.ToArray();
            }
        }

        private static void SetCellValue(object value, IXLCell cell)
        {
            cell.SetValue(value);

            if(value is long)
                cell.Style.NumberFormat.Format = "#,##0";
        }

        public override string MimeType => @"application/vnd.oasis.opendocument.spreadsheet";
        public override string FileExtension => @".xlsx";
    }
}

using OfficeOpenXml;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class ExcelExportFile : ExportFile
    {
        public override byte[] GetFileBytes(string[] headers, object[][] data)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Data");

                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    worksheet.Cells[1, columnIndex + 1].Value = headers[columnIndex];
                    worksheet.Cells[1, columnIndex + 1].Style.Font.Bold = true;
                }

                for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
                {
                    var rowData = data[rowIndex];

                    for (int columnIndex = 0; columnIndex < rowData.Length; columnIndex++)
                    {
                        worksheet.Cells[rowIndex + 2, columnIndex + 1].Value = rowData[columnIndex];
                    }
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    worksheet.Column(columnIndex + 1).AutoFit();
                }

                return excelPackage.GetAsByteArray();
            }
        }

        public override string MimeType => @"application/vnd.oasis.opendocument.spreadsheet";
        public override string FileExtension => @".xlsx";
    }
}
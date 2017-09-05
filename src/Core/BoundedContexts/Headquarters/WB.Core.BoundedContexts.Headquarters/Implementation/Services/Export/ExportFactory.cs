using System;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class ExportFactory : IExportFactory
    {
        public ExportFile CreateExportFile(ExportFileType type)
        {
            switch (type)
            {
                case ExportFileType.Excel:
                    return new ExcelExportFile();
                case ExportFileType.Csv:
                    return new CsvExportFile();
                    case ExportFileType.Tab:
                        return new TabExportFile();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
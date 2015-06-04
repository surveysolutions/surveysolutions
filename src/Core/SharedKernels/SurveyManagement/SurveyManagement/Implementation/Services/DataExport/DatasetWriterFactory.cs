using StatData.Writers;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class DatasetWriterFactory : IDatasetWriterFactory
    {
        public IDatasetWriter CreateDatasetWriter(ExportDataType exportDataType)
        {
            IDatasetWriter writer;
            switch (exportDataType)
            {
                case ExportDataType.Stata:
                    writer = new StataWriter();
                    break;
                case ExportDataType.Spss:
                    writer = new SpssWriter();
                    break;
                case ExportDataType.Tab:
                default:
                    writer = new TabWriter();
                    break;
            }

            return writer;
        }
    }
}
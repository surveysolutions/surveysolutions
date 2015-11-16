using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class DatasetWriterFactory : IDatasetWriterFactory
    {
        public IDatasetWriter CreateDatasetWriter(DataExportFormat format)
        {
            IDatasetWriter writer;
            switch (format)
            {
                case DataExportFormat.STATA:
                    writer = new StataWriter();
                    break;
                case DataExportFormat.SPSS:
                    writer = new SpssWriter();
                    break;
                case DataExportFormat.Tabular:
                default:
                    writer = new TabWriter();
                    break;
            }

            return writer;
        }
    }
}
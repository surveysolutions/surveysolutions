using StatData.Writers;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Services.Implementation
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
                default:
                    writer = new TabWriter();
                    break;
            }

            return writer;
        }
    }
}

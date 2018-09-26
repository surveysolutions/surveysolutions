using StatData.Writers;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Services
{
    internal interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(DataExportFormat format);
    }
}
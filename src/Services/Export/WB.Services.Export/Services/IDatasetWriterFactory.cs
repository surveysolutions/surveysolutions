using StatData.Writers;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Services
{
    public interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(DataExportFormat format);
    }
}
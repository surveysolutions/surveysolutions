using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(DataExportFormat format);
    }
}

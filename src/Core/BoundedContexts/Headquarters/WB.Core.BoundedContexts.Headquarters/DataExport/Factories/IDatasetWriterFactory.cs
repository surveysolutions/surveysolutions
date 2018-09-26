using System;
using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    [Obsolete("KP-11815")]
    internal interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(DataExportFormat format);
    }
}

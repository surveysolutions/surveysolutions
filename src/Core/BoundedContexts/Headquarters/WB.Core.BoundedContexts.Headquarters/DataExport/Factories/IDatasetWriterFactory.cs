using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(DataExportFormat format);
    }
}

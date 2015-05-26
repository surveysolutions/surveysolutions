using StatData.Writers;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    public interface IDatasetWriterFactory
    {
        IDatasetWriter CreateDatasetWriter(ExportDataType writerType);
    }
}

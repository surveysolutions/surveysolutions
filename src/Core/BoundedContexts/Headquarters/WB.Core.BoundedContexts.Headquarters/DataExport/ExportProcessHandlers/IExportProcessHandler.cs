using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: IDataExportProcessDetails
    {
        void ExportData(TProcess process);
    }
}
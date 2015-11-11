using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: IDataExportProcess
    {
        void ExportData(TProcess process);
    }
}
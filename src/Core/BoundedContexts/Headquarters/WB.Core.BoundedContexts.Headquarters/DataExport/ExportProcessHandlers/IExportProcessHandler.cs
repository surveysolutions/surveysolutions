using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: IQueuedProcess
    {
        void ExportData(TProcess process);
    }
}
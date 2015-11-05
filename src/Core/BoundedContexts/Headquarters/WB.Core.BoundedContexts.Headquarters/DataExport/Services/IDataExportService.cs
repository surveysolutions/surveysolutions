using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IDataExportService
    {
        void ExportData(IQueuedProcess process);
    }
}
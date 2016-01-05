using System;
using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    [DisallowConcurrentExecution]
    internal class ExportJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            WB.Core.Infrastructure.Storage.ThreadMarkerManager.MarkCurrentThreadAsIsolated();
            WB.Core.Infrastructure.Storage.ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
            try
            {
                ServiceLocator.Current.GetInstance<IDataExporter>().RunPendingExport();
            }
            catch (Exception exc)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error("Export job failed", exc);
            }
            finally
            {
                WB.Core.Infrastructure.Storage.ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                WB.Core.Infrastructure.Storage.ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
            }
        }
    }
}
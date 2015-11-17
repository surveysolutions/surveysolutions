using System;
using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    [DisallowConcurrentExecution]
    internal class ExportJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
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
                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}
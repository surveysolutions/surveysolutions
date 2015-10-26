using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    [DisallowConcurrentExecution]
    internal class DataExportJob : IJob
    {
        IDataExporter DataExporter
        {
            get { return ServiceLocator.Current.GetInstance<IDataExporter>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            DataExporter.StartDataExport();
        }
    }
}
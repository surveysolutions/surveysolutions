using System;
using System.Diagnostics;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs
{
    [DisallowConcurrentExecution]
    internal class RefreshReportsJob : IJob
    {
        static readonly Gauge jobRunGauge = new Gauge("wb_report_refresh_duration_seconds",
            "Duration of particular report to refresh",
            "ReportName");

        public void Execute(IJobExecutionContext context)
        {
            LogInfo("Started");

            try
            {
                var globalJobRun = Stopwatch.StartNew();

                var task = ServiceLocator.Current.GetInstance<IRefreshReportsTask>();
                var repository = ServiceLocator.Current.GetInstance<IInterviewReportDataRepository>();
                var localJobRun = Stopwatch.StartNew();
                task.RegisterJobStart(DateTime.UtcNow);
                LogInfo($"Start refresh of report");
                repository.Refresh();
                task.RegisterJobCompletion(DateTime.UtcNow);

                LogInfo($"Completed refesh of reports. Took: {localJobRun.Elapsed:g}");

                LogInfo($"All jobs runnig time: {globalJobRun.Elapsed:g}");
            }
            catch (Exception e)
            {
                this.logger.Error("Error occure during job run: ", e);
            }
            finally
            {
                LogInfo("Completed");
            }
        }

        private void LogInfo(string message) => this.logger.Info("Refresh report job: " + message);

        private ILogger logger =>
            ServiceLocator.Current.GetInstance<ILoggerProvider>()
                .GetFor<RefreshReportsJob>();

        private IPlainTransactionManager transactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();
    }
}

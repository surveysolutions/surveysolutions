using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs
{
    public class BackgroundRefreshReportsTask : IRefreshReportsTask
    {
        private const string GroupName = "Refresh Reports";
        readonly JobKey RefreshReportJobKey = new JobKey("refresh reports data", GroupName);
        readonly TriggerKey refreshTriggerKey = new TriggerKey("refresh reports data trigger", GroupName);

        readonly IScheduler scheduler;

        public BackgroundRefreshReportsTask(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public async Task Run()
        {
            if (!(await this.scheduler.CheckExists(RefreshReportJobKey)))
            {
                IJobDetail job = JobBuilder.Create<RefreshReportsJob>()
                    .WithIdentity(RefreshReportJobKey)
                    .StoreDurably()
                    .Build();

                await this.scheduler.AddJob(job, true);
            }

            await this.ScheduleDelayedReportsRefresh();
        }

        private async Task ScheduleDelayedReportsRefresh()
        {
            var trigger = await this.scheduler.GetTrigger(refreshTriggerKey);
            if (trigger != null)
            {
                if (!trigger.JobDataMap.ContainsKey("force"))
                {
                    await this.scheduler.UnscheduleJob(this.refreshTriggerKey);
                }
                else
                {
                    return;
                }
            }
            
            await this.scheduler.ScheduleJob(TriggerBuilder.Create()
                .WithIdentity(refreshTriggerKey)
                .ForJob(RefreshReportJobKey)
                .StartAt(DateBuilder.FutureDate(30, IntervalUnit.Second))
                .Build());
        }

        public async Task ForceRefresh()
        {
            await this.scheduler.UnscheduleJob(this.refreshTriggerKey);
            await this.scheduler.ScheduleJob(TriggerBuilder.Create()
                .WithIdentity(refreshTriggerKey)
                .UsingJobData("force", true)
                .ForJob(RefreshReportJobKey)
                .Build());
        }
        
        public async Task ScheduleRefresh()
        {
            var existingTrigger = await this.scheduler.GetTrigger(this.refreshTriggerKey);

            if (existingTrigger != null)
            {
                if (RefreshFinishTime != null && // make sure that we build report at least every hour
                    DateTime.UtcNow - RefreshFinishTime > TimeSpan.FromHours(1)) return;

                if (!existingTrigger.JobDataMap.ContainsKey("force")) // do not kill force trigger
                {
                   await this.scheduler.UnscheduleJob(this.refreshTriggerKey);
                }
            }

            await this.ScheduleDelayedReportsRefresh();
        }

        public void RegisterJobStart(DateTime now)
        {
            this.RefreshStartTime = now;
        }

        public DateTime? RefreshStartTime { get; set; }
        public DateTime? RefreshFinishTime { get; set; }

        public void RegisterJobCompletion(DateTime now)
        {
            this.RefreshStartTime = null;
            this.RefreshFinishTime = now;
        }

        public DateTime? LastRefreshTime()
        {
            return RefreshFinishTime;
        }
        
        public async Task<RefreshReportsState> GetReportState()
        {
            if ((await this.scheduler.GetCurrentlyExecutingJobs())
                .Any(j => j.JobDetail.Key.Equals(RefreshReportJobKey)))
            {
                return RefreshReportsState.Refreshing;
            }

            if ((await this.scheduler.GetTrigger(this.refreshTriggerKey))?.GetNextFireTimeUtc() != null)
            {
                return RefreshReportsState.ScheduledForRefresh;
            }

            return RefreshReportsState.Actual;
        }
    }
}

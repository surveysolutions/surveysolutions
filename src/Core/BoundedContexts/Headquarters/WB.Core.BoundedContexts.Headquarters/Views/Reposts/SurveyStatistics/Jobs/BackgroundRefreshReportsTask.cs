using System;
using System.Linq;
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

        public void Run()
        {
            if (!this.scheduler.CheckExists(RefreshReportJobKey))
            {
                IJobDetail job = JobBuilder.Create<RefreshReportsJob>()
                    .WithIdentity(RefreshReportJobKey)
                    .StoreDurably()
                    .Build();

                this.scheduler.AddJob(job, true);
            }

            this.ScheduleDelayedReportsRefresh();
        }

        private void ScheduleDelayedReportsRefresh()
        {
            var trigger = this.scheduler.GetTrigger(refreshTriggerKey);
            if (trigger != null)
            {
                if (!trigger.JobDataMap.ContainsKey("force"))
                {
                    this.scheduler.UnscheduleJob(this.refreshTriggerKey);
                }
                else
                {
                    return;
                }
            }
            
            this.scheduler.ScheduleJob(TriggerBuilder.Create()
                .WithIdentity(refreshTriggerKey)
                .ForJob(RefreshReportJobKey)
                .StartAt(DateBuilder.FutureDate(30, IntervalUnit.Second))
                .Build());
        }

        public void ForceRefresh()
        {
            this.scheduler.UnscheduleJob(this.refreshTriggerKey);
            this.scheduler.ScheduleJob(TriggerBuilder.Create()
                .WithIdentity(refreshTriggerKey)
                .UsingJobData("force", true)
                .ForJob(RefreshReportJobKey)
                .Build());
        }
        
        public void ScheduleRefresh()
        {
            var existingTrigger = this.scheduler.GetTrigger(this.refreshTriggerKey);

            if (existingTrigger != null)
            {
                if (RefreshFinishTime != null && // make sure that we build report at least every hour
                    DateTime.UtcNow - RefreshFinishTime > TimeSpan.FromHours(1)) return;

                if (!existingTrigger.JobDataMap.ContainsKey("force")) // do not kill force trigger
                {
                    this.scheduler.UnscheduleJob(this.refreshTriggerKey);
                }
            }

            this.ScheduleDelayedReportsRefresh();
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
        
        public RefreshReportsState GetReportState()
        {
            if (this.scheduler.GetCurrentlyExecutingJobs()
                .Any(j => j.JobDetail.Key.Equals(RefreshReportJobKey)))
            {
                return RefreshReportsState.Refreshing;
            }

            if (this.scheduler.GetTrigger(this.refreshTriggerKey)?.GetNextFireTimeUtc() != null)
            {
                return RefreshReportsState.ScheduledForRefresh;
            }

            return RefreshReportsState.Actual;
        }
    }
}

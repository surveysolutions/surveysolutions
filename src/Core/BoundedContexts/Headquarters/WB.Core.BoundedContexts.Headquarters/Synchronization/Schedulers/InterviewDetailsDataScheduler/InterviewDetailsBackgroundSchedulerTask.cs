using System;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsBackgroundSchedulerTask
    {
        private readonly IScheduler scheduler;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;

        public InterviewDetailsBackgroundSchedulerTask(IScheduler scheduler,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (syncPackagesProcessorBackgroundJobSetting == null) throw new ArgumentNullException(nameof(syncPackagesProcessorBackgroundJobSetting));
            this.scheduler = scheduler;
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting;
        }

        public void Configure()
        {
            RunSyncPackagesProcessorBackgroundJob();
            RunSyncPackagesReprocessorBackgroundJob();
        }

        public void RunSyncPackagesProcessorBackgroundJob()
        {
            IJobDetail job = JobBuilder.Create<SyncPackagesProcessorBackgroundJob>()
                .WithIdentity("Capi interview packages sync", "Synchronization")
                .StoreDurably(true)
                .Build();


            if (this.syncPackagesProcessorBackgroundJobSetting.Enabled)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("Capi interview packages sync trigger", "Synchronization")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(this.syncPackagesProcessorBackgroundJobSetting.SynchronizationInterval)
                        .RepeatForever())
                    .Build();

                this.scheduler.ScheduleJob(job, trigger);
            }

            this.scheduler.AddJob(job, true);
        }

        public void RunSyncPackagesReprocessorBackgroundJob()
        {
            IJobDetail job = JobBuilder.Create<SyncPackagesReprocessorBackgroundJob>()
                .WithIdentity("Capi interview packages reprocesing", "Reprocessor")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("Interview packages reproces trigger", "Reprocessor")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(this.syncPackagesProcessorBackgroundJobSetting.SynchronizationInterval)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);
            this.scheduler.AddJob(job, true);
        }
    }
}
using System;
using System.Threading.Tasks;
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
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting ?? throw new ArgumentNullException(nameof(syncPackagesProcessorBackgroundJobSetting));
        }

        public async Task ConfigureAsync()
        {
            await RunSyncPackagesProcessorBackgroundJobAsync();
            await RunSyncPackagesReprocessorBackgroundJobAsync();
        }

        public async Task RunSyncPackagesProcessorBackgroundJobAsync()
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

                await this.scheduler.ScheduleJob(job, trigger);
            }

            await this.scheduler.AddJob(job, true);
        }

        public async Task RunSyncPackagesReprocessorBackgroundJobAsync()
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

            await this.scheduler.ScheduleJob(job, trigger);
            await this.scheduler.AddJob(job, true);
        }
    }
}

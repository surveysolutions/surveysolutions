using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UsersImportTask
    {
        const string GroupName = "Import users";

        readonly JobKey jobKey = new JobKey("import users job", GroupName);
        readonly TriggerKey triggerKey = new TriggerKey("import users trigger", GroupName);

        readonly IScheduler scheduler;

        public UsersImportTask(IScheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Run()
        {
            if (!await this.scheduler.CheckExists(jobKey))
            {
                IJobDetail job = JobBuilder.Create<UsersImportJob>()
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();

                await this.scheduler.AddJob(job, true);
            }
            
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Second))
                .Build();

            await this.scheduler.ScheduleJob(trigger);
        }

        public virtual async Task<bool> IsJobRunning() =>
            (await this.scheduler.GetCurrentlyExecutingJobs()).FirstOrDefault(x => Equals(x.JobDetail.Key, jobKey)) != null;
    }
}

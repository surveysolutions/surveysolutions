#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class ScheduledTask<TJob, TJobData> : IScheduledJob, IScheduledTask<TJob, TJobData>
        where TJob : IJob<TJobData>
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IWorkspaceContextAccessor contextAccessor;

        public ScheduledTask(ISchedulerFactory schedulerFactory, IWorkspaceContextAccessor contextAccessor)
        {
            this.schedulerFactory = schedulerFactory;
            this.contextAccessor = contextAccessor;
        }

        static ScheduledTask()
        {
            var displayName = typeof(TJob).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            var category = typeof(TJob).GetCustomAttribute<CategoryAttribute>()?.Category;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Key = string.IsNullOrWhiteSpace(category)
                    ? new JobKey(displayName)
                    : new JobKey(displayName, category);
            }
            else
            {
                Key = new JobKey(typeof(TJob).Name);
            }

            Description = typeof(TJob).GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        // ReSharper disable once StaticMemberInGenericType
        public static readonly JobKey Key;

        // ReSharper disable once StaticMemberInGenericType
        public static string? Description;

        public async Task Schedule(TJobData data)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            await scheduler.ScheduleJob(GetTrigger(data));
        }

        public async Task<bool> IsJobRunning(CancellationToken cancellationToken = default)
        {
            var scheduler = await this.schedulerFactory.GetScheduler(cancellationToken);
            var executing = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            return executing.Any(x => Equals(x.JobDetail.Key, Key));
        }

        public async Task RegisterJob()
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            await scheduler.AddJob(JobDetails(), true);
        }

        ITrigger GetTrigger(TJobData data)
        {
            var workspace = contextAccessor.CurrentWorkspace();

            var trigger = TriggerBuilder.Create()
                .ForJob(Key)
                .UsingJobData(BaseTask.TaskDataKey, JsonSerializer.Serialize(data));

            if (workspace != null)
            {
                trigger = trigger.UsingJobData(WorkspaceConstants.QuartzJobKey, workspace.Name);
            }

            return trigger.Build();
        }

        IJobDetail JobDetails()
        {
            var job = JobBuilder.Create<TJob>()
                .WithIdentity(Key)
                .StoreDurably()
                .RequestRecovery();

            if (!string.IsNullOrWhiteSpace(Description))
            {
                job = job.WithDescription(Description);
            }

            return job.Build();
        }
    }
}

using System.Text.Json;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public interface IJob<in T> : IJob
    {
        Task Execute(T data, IJobExecutionContext context);
        
        async Task IJob.Execute(IJobExecutionContext context)
        {
            var data = JsonSerializer.Deserialize<T>(context.MergedJobDataMap.GetString(BaseTask.TaskDataKey));
            await Execute(data, context);
        }
    }
}

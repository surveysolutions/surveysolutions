using System;
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
            var dataValue = context.MergedJobDataMap.ContainsKey(BaseTask.TaskDataKey) 
                ? context.MergedJobDataMap.GetString(BaseTask.TaskDataKey)
                : null;
            if(dataValue == null)
                throw new InvalidOperationException("Task data is not found in job context");
            
            var data = JsonSerializer.Deserialize<T>(dataValue);
            await Execute(data, context);
        }
    }
}

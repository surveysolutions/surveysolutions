using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public class BackgroundService<T>
    {
        private readonly IBackgroundJob<T> job;
        private readonly ILogger logger;
        private readonly BufferBlock<T> queue;

        public BackgroundService(IBackgroundJob<T> job, ILogger logger) 
        {
            this.job = job;
            this.logger = logger;

            queue = new BufferBlock<T>();
            Task.Run(async () =>
            {
                while (await queue.OutputAvailableAsync())
                {
                    var item = await queue.ReceiveAsync();
                    try
                    {
                        await this.job.ExecuteAsync(item);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Error during executing background job", e);
                    }
                }
            });
        }

        public void Enqueue(T item) => queue.Post(item);

        public Task StopAsync()
        {
            queue.Complete();
            return queue.Completion;
        }
    }
}

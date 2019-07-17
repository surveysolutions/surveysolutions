using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public class BackgroundService<T>
    {
        private readonly BufferBlock<T> queue;

        public BackgroundService(IBackgroundJob<T> job, ILogger logger) 
        {
            queue = new BufferBlock<T>();
            var thread = new Thread(async () =>
            {
                while (await queue.OutputAvailableAsync())
                {
                    var item = await queue.ReceiveAsync();
                    try
                    {
                        await job.ExecuteAsync(item);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error during executing background job", e);
                    }
                }
            });

            thread.Start();
        }

        public void Enqueue(T item) => this.queue.Post(item);

        public Task StopAsync()
        {
            queue.Complete();
            return queue.Completion;
        }
    }
}

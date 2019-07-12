using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public class BackgroundService<T>
    {
        private readonly IBackgroundJob<T> job;
        private readonly BufferBlock<T> queue;

        public BackgroundService(IBackgroundJob<T> job) 
        {
            this.job = job;

            queue = new BufferBlock<T>();
            Task.Run(async () =>
            {
                while (await queue.OutputAvailableAsync())
                {
                    var item = await queue.ReceiveAsync();
                    await this.job.ExecuteAsync(item);
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

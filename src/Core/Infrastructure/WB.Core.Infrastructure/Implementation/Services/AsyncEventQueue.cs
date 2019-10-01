using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public class AsyncEventQueue : IAsyncEventQueue
    {
        private readonly BufferBlock<IReadOnlyCollection<CommittedEvent>> queue;

        public AsyncEventQueue(IAsyncEventDispatcher job, ILogger logger) 
        {
            queue = new BufferBlock<IReadOnlyCollection<CommittedEvent>>();
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

        public void Enqueue(IReadOnlyCollection<CommittedEvent> item) => this.queue.Post(item);
    }
}

using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IViewModelEventQueue
    {
        void Enqueue(IEnumerable<CommittedEvent> @events);
    }

    internal class ViewModelEventQueue : BackgroundService<IEnumerable<CommittedEvent>>, IViewModelEventQueue
    {
        public ViewModelEventQueue(IBackgroundJob<IEnumerable<CommittedEvent>> job, ILogger logger) : base(job, logger)
        {
        }
    }
}

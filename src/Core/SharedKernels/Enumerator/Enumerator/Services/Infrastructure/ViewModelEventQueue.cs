using Ncqrs.Eventing;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IViewModelEventQueue
    {
        void Enqueue(CommittedEvent @event);
    }

    internal class ViewModelEventQueue : BackgroundService<CommittedEvent>, IViewModelEventQueue
    {
        public ViewModelEventQueue(IBackgroundJob<CommittedEvent> job) : base(job)
        {
        }
    }
}

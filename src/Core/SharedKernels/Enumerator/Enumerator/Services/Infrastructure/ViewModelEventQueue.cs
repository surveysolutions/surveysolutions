using Ncqrs.Eventing;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class ViewModelEventQueue : BackgroundService<CommittedEvent>
    {
        public ViewModelEventQueue(IBackgroundJob<CommittedEvent> job) : base(job)
        {
        }
    }
}

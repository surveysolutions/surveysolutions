using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class HardDeletedInterviewEventHandler : AbstractFunctionalEventHandler<HardDeletedInterview, IReadSideKeyValueStorage<HardDeletedInterview>>, IUpdateHandler<HardDeletedInterview, InterviewHardDeleted>
    {
        public HardDeletedInterviewEventHandler(IReadSideKeyValueStorage<HardDeletedInterview> readSideStorage)
            : base(readSideStorage)
        {
        }

        public HardDeletedInterview Update(HardDeletedInterview state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return new HardDeletedInterview() { InterviewId = @event.EventSourceId };
        }
    }
}

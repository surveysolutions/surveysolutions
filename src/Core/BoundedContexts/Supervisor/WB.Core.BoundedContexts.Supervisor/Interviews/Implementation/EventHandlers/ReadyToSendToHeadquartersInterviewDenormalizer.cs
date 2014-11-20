using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.EventHandlers
{
    internal class ReadyToSendToHeadquartersInterviewDenormalizer : AbstractFunctionalEventHandler<ReadyToSendToHeadquartersInterview>,
        IUpdateHandler<ReadyToSendToHeadquartersInterview, InterviewStatusChanged>
    {
        public ReadyToSendToHeadquartersInterviewDenormalizer(IReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview> readsideRepositoryWriter)
            : base(readsideRepositoryWriter) {}

        public ReadyToSendToHeadquartersInterview Update(ReadyToSendToHeadquartersInterview state, IPublishedEvent<InterviewStatusChanged> @event)
        {
            return @event.Payload.Status == InterviewStatus.ApprovedBySupervisor
                ? new ReadyToSendToHeadquartersInterview(interviewId: @event.EventSourceId)
                : null;
        }
    }
}
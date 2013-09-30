using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CommitDenormalizer : IEventHandler<InterviewRestarted>,
                                      IEventHandler<InterviewSynchronized>,
                                      IEventHandler<InterviewDeclaredValid>, 
                                      IEventHandler<InterviewDeclaredInvalid>
    {
        public CommitDenormalizer(IChangeLogManipulator changeLog)
        {
            this.changeLog = changeLog;
        }

        private readonly IChangeLogManipulator changeLog;

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            changeLog.ReopenDraftRecord(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            changeLog.OpenDraftRecord(evnt.EventSourceId, evnt.EventSequence + 1);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            changeLog.CloseDraftRecord(evnt.EventSourceId, evnt.EventSequence, true);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            changeLog.CloseDraftRecord(evnt.EventSourceId, evnt.EventSequence, false);
        }
    }
}
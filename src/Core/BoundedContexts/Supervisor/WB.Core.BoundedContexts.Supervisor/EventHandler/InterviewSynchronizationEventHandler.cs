using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using InterviewDeleted = Main.Core.Events.Questionnaire.Completed.InterviewDeleted;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSynchronizationEventHandler : IEventHandler<InterviewerAssigned>,
                                                        IEventHandler<InterviewRejected>,
                                                        IEventHandler<InterviewCompleted>,
                                                        IEventHandler<InterviewDeleted>, 
                                                        IEventHandler
    {
        private readonly ISynchronizationDataStorage syncStorage;
        // private readonly IReadSideRepositoryWriter<InterviewSynchronizationDto> interviewWriter;

        public InterviewSynchronizationEventHandler(ISynchronizationDataStorage syncStorage)
        {
            this.syncStorage = syncStorage;
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            syncStorage.SaveInterview(new InterviewSynchronizationDto(), evnt.Payload.UserId);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            syncStorage.SaveInterview(new InterviewSynchronizationDto(), evnt.Payload.UserId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);

        }
        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews { get; private set; }

        public Type[] BuildsViews
        {
            get { return new Type[] {typeof (SynchronizationDelta)}; }
        }
    }
}

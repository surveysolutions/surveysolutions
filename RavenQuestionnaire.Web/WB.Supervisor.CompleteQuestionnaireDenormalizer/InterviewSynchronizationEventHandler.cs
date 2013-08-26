using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Supervisor.CompleteQuestionnaireDenormalizer
{
    internal class InterviewSynchronizationEventHandler : 
                                                        IEventHandler<QuestionnaireAssignmentChanged>,
                                                        IEventHandler<QuestionnaireStatusChanged>,
                                                        IEventHandler<InterviewDeleted>,IEventHandler<InterviewMetaInfoUpdated>, IEventHandler
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> interviewWriter;

        public InterviewSynchronizationEventHandler(ISynchronizationDataStorage syncStorage, IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> interviewWriter)
        {
            this.syncStorage = syncStorage;
            this.interviewWriter = interviewWriter;
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var item = interviewWriter.GetById(evnt.EventSourceId);
            item.Responsible = evnt.Payload.Responsible;
            syncStorage.SaveInterview(item, evnt.Payload.Responsible.Id);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            UpdateInterviewStatus(evnt.EventSourceId, evnt.Payload.Status);
        }

        private void UpdateInterviewStatus(Guid interviewId,SurveyStatus status)
        {
            var item = interviewWriter.GetById(interviewId);
            if (SurveyStatus.IsStatusAllowDownSupervisorSync(status))
            {
                item.Status = status;
                syncStorage.SaveInterview(item, item.Responsible.Id);
            }
            else
                syncStorage.MarkInterviewForClientDeleting(interviewId, item.Responsible.Id);
        }

        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            UpdateInterviewStatus(evnt.EventSourceId, SurveyStatus.GetStatusByIdOrDefault(evnt.Payload.StatusId));
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[] {typeof (ZipView)}; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(SynchronizationDelta) }; }
        }

       
    }
}

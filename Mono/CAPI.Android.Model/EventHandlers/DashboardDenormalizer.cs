using System;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer :
                                      IEventHandler<InterviewMetaInfoUpdated>,
                                      IEventHandler<InterviewCompleted>,
                                      IEventHandler<InterviewRestarted>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtOdocumentStorage;

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
            IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage
            )
        {
            questionnaireDtOdocumentStorage = questionnaireDTOdocumentStorage;
            surveyDtOdocumentStorage = surveyDTOdocumentStorage;
        }

        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            var meta = evnt.Payload;

            if (!IsVisible(meta.Status))
            {
                questionnaireDtOdocumentStorage.Remove(evnt.EventSourceId);
                return;
            }

            var items = meta.FeaturedQuestionsMeta.Select(q => new FeaturedItem(q.PublicKey, q.Title, q.Value)).ToList();
            var survey = surveyDtOdocumentStorage.GetById(meta.QuestionnaireId);
            
            if (survey == null)
                surveyDtOdocumentStorage.Store(new SurveyDto(meta.QuestionnaireId, "test questionnarie"), meta.QuestionnaireId);
          
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(evnt.EventSourceId, meta.UserId, meta.QuestionnaireId, meta.Status.ToString(), items),
                evnt.EventSourceId);
        }

        protected bool IsVisible(InterviewStatus status)
        {
            return status == InterviewStatus.InterviewerAssigned || status == InterviewStatus.RejectedBySupervisor ||
                   status == InterviewStatus.Completed ||
                   status == InterviewStatus.Restarted;
        }
        
        public void RemoveItem(Guid itemId)
        {
            questionnaireDtOdocumentStorage.Remove(itemId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = SurveyStatus.Complete.PublicId.ToString();

            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = SurveyStatus.Reinit.PublicId.ToString();

            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }
    }
}
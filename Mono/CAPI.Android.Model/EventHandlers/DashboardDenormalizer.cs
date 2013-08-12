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
            var status = SurveyStatus.GetStatusByIdOrDefault(meta.StatusId);

            if (!IsVisible(status))
            {
                questionnaireDtOdocumentStorage.Remove(evnt.EventSourceId);
                return;
            }

            var items = meta.FeaturedQuestionsMeta.Select(q => new FeaturedItem(q.PublicKey, q.Title, q.Value)).ToList();
            var survey = surveyDtOdocumentStorage.GetById(meta.TemplateId);
            
            if (survey == null)
                surveyDtOdocumentStorage.Store(new SurveyDto(meta.TemplateId, meta.Title), meta.TemplateId);
          
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(evnt.EventSourceId, meta.ResponsibleId.Value, meta.TemplateId, status, items),
                evnt.EventSourceId);
        }

        protected bool IsVisible(SurveyStatus status)
        {
            return status == SurveyStatus.Initial || status == SurveyStatus.Redo || status == SurveyStatus.Complete ||
                   status == SurveyStatus.Reinit || status == SurveyStatus.Error;
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
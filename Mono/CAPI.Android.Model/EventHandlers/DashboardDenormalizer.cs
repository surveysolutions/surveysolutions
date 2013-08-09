using System;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<NewAssigmentCreated>,
                                         IEventHandler<QuestionnaireStatusChanged>, IEventHandler<InterviewMetaInfoUpdated>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> _questionnaireDTOdocumentStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> _surveyDTOdocumentStorage;

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
            IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage
            )
        {
            _questionnaireDTOdocumentStorage = questionnaireDTOdocumentStorage;
            _surveyDTOdocumentStorage = surveyDTOdocumentStorage;
        }

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            var document = evnt.Payload.Source;
            ProcessCompleteQuestionnaire(document);
        }

        #endregion
        protected void ProcessCompleteQuestionnaire( CompleteQuestionnaireDocument doc)
        {
            if (!IsVisible(doc.Status))
            {
                _questionnaireDTOdocumentStorage.Remove(doc.PublicKey);
                return;
            }
            var featuredItems = doc.GetFeaturedQuestions();
            var items = featuredItems.Select(
                q =>
                new FeaturedItem(q.PublicKey, q.QuestionText,
                                 q.GetAnswerString())).ToList();
            var survey = _surveyDTOdocumentStorage.GetById(doc.TemplateId);
            if (survey == null)
                _surveyDTOdocumentStorage.Store(new SurveyDto(doc.TemplateId, doc.Title), doc.TemplateId);
          
            _questionnaireDTOdocumentStorage.Store(
                new QuestionnaireDTO(doc.PublicKey, doc.Responsible.Id, doc.TemplateId, doc.Status, items),
                doc.PublicKey);
        }


        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            var meta = evnt.Payload;
            var status = SurveyStatus.GetStatusByIdOrDefault(meta.StatusId);

            if (!IsVisible(status))
            {
                _questionnaireDTOdocumentStorage.Remove(evnt.EventSourceId);
                return;
            }

            var items = meta.FeaturedQuestionsMeta.Select(q => new FeaturedItem(q.PublicKey, q.Title, q.Value)).ToList();
            var survey = _surveyDTOdocumentStorage.GetById(meta.TemplateId);
            
            if (survey == null)
                _surveyDTOdocumentStorage.Store(new SurveyDto(meta.TemplateId, meta.Title), meta.TemplateId);
          
            _questionnaireDTOdocumentStorage.Store(
                new QuestionnaireDTO(evnt.EventSourceId, meta.ResponsibleId.Value, meta.TemplateId, status, items),
                evnt.EventSourceId);
        }

 
        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var questionnaire = _questionnaireDTOdocumentStorage.GetById(evnt.Payload.CompletedQuestionnaireId);
            if(questionnaire==null)
                return;
            if (!IsVisible(evnt.Payload.Status))
            {
                _questionnaireDTOdocumentStorage.Remove(evnt.EventSourceId);
            }
            questionnaire.Status = evnt.Payload.Status.PublicId.ToString();

            _questionnaireDTOdocumentStorage.Store(questionnaire, evnt.Payload.CompletedQuestionnaireId);
          
        }

        #endregion
        protected bool IsVisible(SurveyStatus status)
        {
            return status == SurveyStatus.Initial || status == SurveyStatus.Redo || status == SurveyStatus.Complete ||
                   status == SurveyStatus.Reinit || status == SurveyStatus.Error;
        }
        
        public void RemoveItem(Guid itemId)
        {
            _questionnaireDTOdocumentStorage.Remove(itemId);
        }
    }
}
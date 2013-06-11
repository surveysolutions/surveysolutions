using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire.Completed;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;


namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<NewAssigmentCreated>, IEventHandler<QuestionnaireStatusChanged>, IEventHandler<CompleteQuestionnaireDeleted>
    {
        private readonly IDenormalizerStorage<QuestionnaireDTO> _questionnaireDTOdocumentStorage;
        private readonly IDenormalizerStorage<SurveyDto> _surveyDTOdocumentStorage;

        public DashboardDenormalizer(IDenormalizerStorage<QuestionnaireDTO> questionnaireDTOdocumentStorage,
            IDenormalizerStorage<SurveyDto> surveyDTOdocumentStorage
            )
        {
            _questionnaireDTOdocumentStorage = questionnaireDTOdocumentStorage;
            _surveyDTOdocumentStorage = surveyDTOdocumentStorage;
        }

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            var document = evnt.Payload.Source;
            PropeedCompleteQuestionnaire(document);
        }

        #endregion
        protected void PropeedCompleteQuestionnaire( CompleteQuestionnaireDocument doc)
        {
            if (!IsVisible(doc.Status))
            {
                _questionnaireDTOdocumentStorage.Remove(doc.PublicKey);
                return;
            }
            var featuredItems = doc.Find<ICompleteQuestion>(q => q.Featured);
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

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            _questionnaireDTOdocumentStorage.Remove(evnt.EventSourceId);
        }
    }
}
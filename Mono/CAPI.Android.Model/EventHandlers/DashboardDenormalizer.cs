using System;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
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
                                      IEventHandler<InterviewRestarted>, 
                                      IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> questionnaireStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtOdocumentStorage;

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
            IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage, IReadSideRepositoryWriter<QuestionnaireDocument> questionnaireStorage
            )
        {
            this.questionnaireDtOdocumentStorage = questionnaireDTOdocumentStorage;
            this.surveyDtOdocumentStorage = surveyDTOdocumentStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            var questionnarieTemplate = questionnaireStorage.GetById(evnt.Payload.QuestionnaireId);
            if(questionnarieTemplate==null)
                return;
            var items =
                evnt.Payload.FeaturedQuestionsMeta.Select(
                    q =>
                    new FeaturedItem(q.Id, questionnarieTemplate.Find<IQuestion>(q.Id).QuestionText, q.Answer.ToString()))
                    .ToList();
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.Status, items),
                evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = (int)InterviewStatus.Completed;

            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = (int)InterviewStatus.Restarted;

            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            surveyDtOdocumentStorage.Store(new SurveyDto(evnt.EventSourceId, evnt.Payload.Source.Title), evnt.EventSourceId);
        }
    }
}
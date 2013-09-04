using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer :
                                      IEventHandler<SynchronizationMetadataApplied>,
         IEventHandler<InterviewSynchronized>,
                                      IEventHandler<InterviewCompleted>,
                                      IEventHandler<InterviewRestarted>, 
                                      IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtOdocumentStorage;

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
                                     IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage, 
                                     IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireStorage)
        {
            this.questionnaireDtOdocumentStorage = questionnaireDTOdocumentStorage;
            this.surveyDtOdocumentStorage = surveyDTOdocumentStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.Status, evnt.Payload.FeaturedQuestionsMeta);
        }

        private void AddOrUpdateInterviewToDashboard(Guid questionnaireId, Guid interviewId, Guid responsibleId,
                                                     InterviewStatus status,
                                                     IEnumerable<AnsweredQuestionSynchronizationDto>
                                                         answeredQuestions)
        {
            var questionnarieTemplate = questionnaireStorage.GetById(questionnaireId);
            if (questionnarieTemplate == null)
                return;
            var items =
                FilterNonFeaturedQuestionsByTemplate(questionnarieTemplate.Questionnaire, answeredQuestions).Select(
                    q =>
                    new FeaturedItem(q.Id, questionnarieTemplate.Questionnaire.Find<IQuestion>(q.Id).QuestionText,
                                     q.Answer.ToString()))
                                                                                                            .ToList();
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(interviewId, responsibleId, questionnaireId, status,
                                     items), interviewId);
        }

        private IEnumerable<AnsweredQuestionSynchronizationDto> FilterNonFeaturedQuestionsByTemplate(
            QuestionnaireDocument template, IEnumerable<AnsweredQuestionSynchronizationDto> allQuestions)
        {
            return allQuestions.Where(
                q =>
                template.FirstOrDefault<IQuestion>(
                    questionFromTemplate => questionFromTemplate.PublicKey == q.Id && questionFromTemplate.Featured) !=
                null);
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.InterviewData.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId,
                                            evnt.Payload.InterviewData.Status, evnt.Payload.InterviewData.Answers);
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
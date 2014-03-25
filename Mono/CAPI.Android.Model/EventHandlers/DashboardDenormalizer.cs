using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer :                                            
                                      BaseDenormalizer,
                                      IEventHandler,
                                      IEventHandler<SynchronizationMetadataApplied>,
                                      IEventHandler<InterviewSynchronized>,
                                      IEventHandler<InterviewDeclaredValid>,
                                      IEventHandler<InterviewDeclaredInvalid>,
                                      IEventHandler<InterviewStatusChanged>, 
                                      IEventHandler<TemplateImported>,

                                      IEventHandler<InterviewOnClientCreated>,
                                      IEventHandler<InterviewerAssigned>,
                                      IEventHandler<SupervisorAssigned>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtOdocumentStorage;
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

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
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.Status, 
                evnt.Payload.FeaturedQuestionsMeta, false);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId, InterviewStatus.InterviewerAssigned, 
                new AnsweredQuestionSynchronizationDto[0], true);
        }


        private void AddOrUpdateInterviewToDashboard(Guid questionnaireId, Guid interviewId, Guid responsibleId,
                                                     InterviewStatus status,IEnumerable<AnsweredQuestionSynchronizationDto>answeredQuestions, 
                                                     bool createdOnClient)
        {
            var questionnaireTemplate = questionnaireStorage.GetById(questionnaireId);
            if (questionnaireTemplate == null)
                return;

            var items =
                FilterNonFeaturedQuestionsByTemplate(questionnaireTemplate.Questionnaire, answeredQuestions).Select(
                    q => CreateFeaturedItem(q, questionnaireTemplate)).ToList();

            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(interviewId, responsibleId, questionnaireId, status,
                                     items, createdOnClient), interviewId);
        }

        private FeaturedItem CreateFeaturedItem(AnsweredQuestionSynchronizationDto q, QuestionnaireDocumentVersioned questionnaireTemplate)
        {
            var featuredQuestion = questionnaireTemplate.Questionnaire.Find<IQuestion>(q.Id);
            if (featuredQuestion == null)
                return null;
            
            var answerString = q.Answer.ToString();

            if (questionTypesWithOptions.Contains(featuredQuestion.QuestionType))
            {
                var answerValues = QuestionUtils.ExtractSelectedOptions(answerString);

                var options =
                    featuredQuestion.Answers.Where(o => answerValues.Contains(decimal.Parse(o.AnswerValue))).Select(o => o.AnswerText);
                
                answerString = string.Join(",", options);
            }

            return new FeaturedItem(q.Id, featuredQuestion.QuestionText, answerString);
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
                                            evnt.Payload.InterviewData.Status, evnt.Payload.InterviewData.Answers, false);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            surveyDtOdocumentStorage.Store(new SurveyDto(evnt.EventSourceId, evnt.Payload.Source.Title), evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Valid = true;
            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            var questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Valid = false;
            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if(!IsInterviewCompletedOrRestarted(evnt.Payload.Status))
                return;

            QuestionnaireDTO questionnaire = questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = (int)evnt.Payload.Status;
            questionnaire.Comments = evnt.Payload.Comment;

            questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        private bool IsInterviewCompletedOrRestarted(InterviewStatus status)
        {
            return status == InterviewStatus.Completed || status == InterviewStatus.Restarted;
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireDTO) }; }
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            //do nothing
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            //do nothing
        }
    }
}
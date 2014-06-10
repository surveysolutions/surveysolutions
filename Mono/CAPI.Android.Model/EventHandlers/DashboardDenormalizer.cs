using System;
using System.Collections.Generic;
using System.Globalization;
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
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
                                      IEventHandler<PlainQuestionnaireRegistered>,

                                      IEventHandler<TextQuestionAnswered>,
                                      IEventHandler<MultipleOptionsQuestionAnswered>,
                                      IEventHandler<SingleOptionQuestionAnswered>,
                                      IEventHandler<NumericRealQuestionAnswered>,
                                      IEventHandler<NumericQuestionAnswered>,
                                      IEventHandler<NumericIntegerQuestionAnswered>,
                                      IEventHandler<DateTimeQuestionAnswered>,
                                      IEventHandler<GeoLocationQuestionAnswered>,
                                      IEventHandler<QRBarcodeQuestionAnswered>,

                                      IEventHandler<AnswerRemoved>,

                                      IEventHandler<InterviewOnClientCreated>,
                                      IEventHandler<InterviewerAssigned>,
                                      IEventHandler<SupervisorAssigned>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtoDocumentStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
                                     IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage, 
                                     IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireStorage,
                                     IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnaireDtOdocumentStorage = questionnaireDTOdocumentStorage;
            this.surveyDtoDocumentStorage = surveyDTOdocumentStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.Status,
                evnt.Payload.FeaturedQuestionsMeta, evnt.Payload.CreatedOnClient, false);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, evnt.EventSourceId, evnt.Payload.UserId, InterviewStatus.InterviewerAssigned, 
                new AnsweredQuestionSynchronizationDto[0], true, true);
        }


        private void AddOrUpdateInterviewToDashboard(Guid questionnaireId, Guid interviewId, Guid responsibleId,
                                                     InterviewStatus status, IEnumerable<AnsweredQuestionSynchronizationDto>answeredQuestions, 
                                                     bool createdOnClient, bool canBeDeleted)
        {
            var questionnaireTemplate = questionnaireStorage.GetById(questionnaireId);
            if (questionnaireTemplate == null)
                return;

            var items = new List<FeaturedItem>();
            foreach (var featuredQuestion in questionnaireTemplate.Questionnaire.Find<IQuestion>(q => q.Featured))
            {
                var item = answeredQuestions.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);
                items.Add(CreateFeaturedItem(featuredQuestion.PublicKey, item != null && item.Answer!=null ? item.Answer.ToString() : string.Empty, featuredQuestion));
            }
            
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(interviewId, responsibleId, questionnaireId, status,
                                     items, questionnaireTemplate.Version, createdOnClient, canBeDeleted), interviewId);
        }

        private FeaturedItem CreateFeaturedItem(Guid questionId, string answerString, IQuestion featuredQuestion)
        {
            if (!questionTypesWithOptions.Contains(featuredQuestion.QuestionType))
                return new FeaturedItem(questionId, featuredQuestion.QuestionText, answerString);

            var answerValues = QuestionUtils.ExtractSelectedOptions(answerString);
            if (answerValues != null && answerValues.Length > 0)
            {
                var options =
                    featuredQuestion.Answers.Where(o => answerValues.Contains(decimal.Parse(o.AnswerValue)))
                        .Select(o => o.AnswerText);

                answerString = string.Join(",", options);
            }
            else
            {
                answerString = string.Empty;
            }

            return new FeaturedCategoricalItem(questionId, featuredQuestion.QuestionText, answerString,
                featuredQuestion.Answers.Select(
                    answer =>
                        new FeaturedCategoricalOption()
                        {
                            OptionValue = decimal.Parse(answer.AnswerValue),
                            OptionText = answer.AnswerText
                        }));
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

        private string getAnswerForCategoricalSingleQuestion(FeaturedItem featuredQuestion, decimal answer)
        {
            var selectedOptionText = answer.ToString(CultureInfo.InvariantCulture);

            var categoricalFeaturedQuestion = featuredQuestion as FeaturedCategoricalItem;
            if (categoricalFeaturedQuestion != null)
            {
                var selectedOption = categoricalFeaturedQuestion.Options.FirstOrDefault(option => option.OptionValue == answer);
                if (selectedOption != null)
                {
                    selectedOptionText = selectedOption.OptionText;
                }
            }

            return selectedOptionText;
        }

        private string getAnswerForCategoricalMultiQuestion(FeaturedItem featuredQuestion, decimal[] answer)
        {
            var selectedOptionsText = string.Join(", ", answer);

            var categoricalFeaturedQuestion = featuredQuestion as FeaturedCategoricalItem;
            if (categoricalFeaturedQuestion != null)
            {
                var selectedOptions =
                    categoricalFeaturedQuestion.Options.Where(option => answer.Contains(option.OptionValue))
                        .Select(option => option.OptionText);
                if (selectedOptions.Any())
                {
                    selectedOptionsText = string.Join(", ", selectedOptions);
                }
            }

            return selectedOptionsText;
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
                                            evnt.Payload.InterviewData.Status, evnt.Payload.InterviewData.Answers, 
                                            evnt.Payload.InterviewData.CreatedOnClient, false);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreSurveyDto(id, questionnaireDocument, version);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreSurveyDto(id, questionnaireDocument, evnt.Payload.Version);
        }

        private void StoreSurveyDto(Guid id, QuestionnaireDocument questionnaireDocument, long version)
        {
            this.surveyDtoDocumentStorage.Store(new SurveyDto(id, questionnaireDocument.Title, version), id);
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

        private void AnswerQuestion(Guid interviewId, Guid questionId, Func<FeaturedItem, string> getAnswer)
        {
            QuestionnaireDTO questionnaire = questionnaireDtOdocumentStorage.GetById(interviewId);

            if (questionnaire == null)
                return;

            var properties = questionnaire.GetProperties();
            int keyIndex = Array.FindIndex(properties, w => w.PublicKey == questionId);

            if (keyIndex >= 0)
            {
                var featuredQuestion = properties[keyIndex];

                featuredQuestion.Value = getAnswer(featuredQuestion);

                questionnaire.SetProperties(properties);
                questionnaireDtOdocumentStorage.Store(questionnaire, interviewId);
            }
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, (featuredQuestion) => evnt.Payload.Answer);
        }

        
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) =>
                    getAnswerForCategoricalMultiQuestion(featuredQuestion, evnt.Payload.SelectedValues));
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) =>
                    getAnswerForCategoricalSingleQuestion(featuredQuestion, evnt.Payload.SelectedValue));
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) => evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) => evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) => evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) => evnt.Payload.Answer.ToString("d", CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                (featuredQuestion) =>
                    string.Format("{0},{1}[{2}]", evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, (featuredQuestion) => evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, (featuredQuestion) => string.Empty);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;

using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.Capi.ViewModel.Dashboard;

namespace WB.UI.Capi.EventHandlers
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
                                      IEventHandler<QuestionnaireDeleted>,
                                      IEventHandler<PlainQuestionnaireRegistered>,

                                      IEventHandler<TextQuestionAnswered>,
                                      IEventHandler<MultipleOptionsQuestionAnswered>,
                                      IEventHandler<SingleOptionQuestionAnswered>,
                                      IEventHandler<NumericRealQuestionAnswered>,
                                      IEventHandler<NumericIntegerQuestionAnswered>,
                                      IEventHandler<DateTimeQuestionAnswered>,
                                      IEventHandler<GeoLocationQuestionAnswered>,
                                      IEventHandler<QRBarcodeQuestionAnswered>,

                                      IEventHandler<AnswersRemoved>,

                                      IEventHandler<InterviewOnClientCreated>,
                                      IEventHandler<InterviewerAssigned>,
                                      IEventHandler<SupervisorAssigned>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtOdocumentStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStorage;
        private readonly IReadSideRepositoryWriter<SurveyDto> surveyDtoDocumentStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption, QuestionType.DropDownList, QuestionType.YesNo };

        public DashboardDenormalizer(IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDTOdocumentStorage,
                                     IReadSideRepositoryWriter<SurveyDto> surveyDTOdocumentStorage,
                                     IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStorage,
                                     IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnaireDtOdocumentStorage = questionnaireDTOdocumentStorage;
            this.surveyDtoDocumentStorage = surveyDTOdocumentStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new object[] { this.questionnaireDtOdocumentStorage, this.surveyDtoDocumentStorage}; }
        }

        public override object[] Readers
        {
            get { return new object[] { this.questionnaireDtOdocumentStorage}; }
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, 
                evnt.Payload.QuestionnaireVersion, 
                evnt.EventSourceId, 
                evnt.Payload.UserId, 
                evnt.Payload.Status, 
                evnt.Payload.Comments,
                evnt.Payload.FeaturedQuestionsMeta, 
                evnt.Payload.CreatedOnClient, 
                false);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion, 
                evnt.EventSourceId, 
                evnt.Payload.UserId, 
                InterviewStatus.InterviewerAssigned, 
                null,
                new AnsweredQuestionSynchronizationDto[0], 
                true, 
                true);
        }


        private void AddOrUpdateInterviewToDashboard(Guid questionnaireId, 
            long questionnaireVersion, 
            Guid interviewId, 
            Guid responsibleId,
            InterviewStatus status, 
            string comments, 
            IEnumerable<AnsweredQuestionSynchronizationDto>answeredQuestions,
            bool createdOnClient, 
            bool canBeDeleted)
        {
            var questionnaireTemplate = this.questionnaireStorage.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
            if (questionnaireTemplate == null)
                return;

            var items = new List<FeaturedItem>();
            foreach (var featuredQuestion in questionnaireTemplate.Questionnaire.Find<IQuestion>(q => q.Featured))
            {
                var item = answeredQuestions.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);
                items.Add(this.CreateFeaturedItem(featuredQuestion, item == null ? null : item.Answer));
            }
            
            this.questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(interviewId, responsibleId, questionnaireId, status,
                                     items, questionnaireTemplate.Version, comments, createdOnClient, canBeDeleted), interviewId);
        }

        private FeaturedItem CreateFeaturedItem(IQuestion featuredQuestion, object answer)
        {
            if (this.questionTypesWithOptions.Contains(featuredQuestion.QuestionType))
            {
                var featuredCategoricalOptions = featuredQuestion.Answers.Select(
                    option =>
                        new FeaturedCategoricalOption
                        {
                            OptionValue = decimal.Parse(option.AnswerValue, CultureInfo.InvariantCulture),
                            OptionText = option.AnswerText,
                        });
                if (answer == null)
                    return new FeaturedCategoricalItem(featuredQuestion.PublicKey, featuredQuestion.QuestionText,
                        string.Empty, featuredCategoricalOptions);


                object objectAnswer;

                if (answer.GetType().IsArray)
                {
                    objectAnswer = (answer as object[]).Select(x => Convert.ToDecimal(x, CultureInfo.InvariantCulture)).ToArray();
                }
                else
                {
                    objectAnswer = Convert.ToDecimal(answer, CultureInfo.InvariantCulture);
                }

                return new FeaturedCategoricalItem(featuredQuestion.PublicKey, 
                    featuredQuestion.QuestionText,
                    AnswerUtils.AnswerToString(objectAnswer,
                        (optionValue) => getCategoricalAnswerOptionText(featuredCategoricalOptions, optionValue)),
                    featuredCategoricalOptions);
            }

            if (featuredQuestion.QuestionType == QuestionType.DateTime && answer is string)
            {
                answer = DateTime.Parse((string) answer).ToLocalTime();
            }

            return new FeaturedItem(featuredQuestion.PublicKey, featuredQuestion.QuestionText,
                AnswerUtils.AnswerToString(answer));
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.InterviewData.QuestionnaireId,
                evnt.Payload.InterviewData.QuestionnaireVersion, 
                evnt.EventSourceId, 
                evnt.Payload.UserId,
                evnt.Payload.InterviewData.Status,
                evnt.Payload.InterviewData.Comments, 
                evnt.Payload.InterviewData.Answers, 
                evnt.Payload.InterviewData.CreatedOnClient, 
                canBeDeleted: false);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreSurveyDto(id, questionnaireDocument, version, evnt.Payload.AllowCensusMode);
        }


        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.QuestionnaireVersion;

            this.RemoveSurveyDto(id, version);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreSurveyDto(id, questionnaireDocument, evnt.Payload.Version, evnt.Payload.AllowCensusMode);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            var questionnaire = this.questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Valid = true;
            this.questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            var questionnaire = this.questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Valid = false;
            this.questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if(!this.IsInterviewCompletedOrRestarted(evnt.Payload.Status))
                return;

            QuestionnaireDTO questionnaire = this.questionnaireDtOdocumentStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.Status = (int)evnt.Payload.Status;
            questionnaire.Comments = evnt.Payload.Comment;

            this.questionnaireDtOdocumentStorage.Store(questionnaire, evnt.EventSourceId);
        }

        private bool IsInterviewCompletedOrRestarted(InterviewStatus status)
        {
            return status == InterviewStatus.Completed || status == InterviewStatus.Restarted;
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            //do nothing
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            //do nothing
        }

        private void AnswerQuestion(Guid interviewId, Guid questionId, object answer)
        {
            QuestionnaireDTO questionnaire = this.questionnaireDtOdocumentStorage.GetById(interviewId);

            if (questionnaire == null) return;

            var featuredItems = questionnaire.GetProperties().ToList();

            var preFilledQuestion = featuredItems.FirstOrDefault(question => question.PublicKey == questionId);

            if (preFilledQuestion == null) return;

            preFilledQuestion.Value = this.getAnswer(preFilledQuestion, answer);

            questionnaire.SetProperties(featuredItems);

            this.questionnaireDtOdocumentStorage.Store(questionnaire, interviewId);
        }

        private string getAnswer(FeaturedItem featuredQuestion, object answer)
        {
            var featuredCategoricalQuestion = featuredQuestion as FeaturedCategoricalItem;
            if (featuredCategoricalQuestion != null)
                return AnswerUtils.AnswerToString(Convert.ToDecimal(answer, CultureInfo.InvariantCulture),
                    (optionValue) => getCategoricalAnswerOptionText(featuredCategoricalQuestion.Options, optionValue));
            return AnswerUtils.AnswerToString(answer);
        }

        private static string getCategoricalAnswerOptionText(
            IEnumerable<FeaturedCategoricalOption> featuredCategoricalOptions, decimal optionValue)
        {
            var featuredCategoricalOption =
                featuredCategoricalOptions.FirstOrDefault(option => option.OptionValue == optionValue);

            return featuredCategoricalOption != null ? featuredCategoricalOption.OptionText : string.Empty;
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValue);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                new GeoPosition(latitude: evnt.Payload.Latitude, longitude: evnt.Payload.Longitude,
                    accuracy: evnt.Payload.Accuracy, altitude:evnt.Payload.Altitude, 
                    timestamp: evnt.Payload.Timestamp));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.AnswerQuestion(evnt.EventSourceId, question.Id, string.Empty);
            }
        }

        private void StoreSurveyDto(Guid id, QuestionnaireDocument questionnaireDocument, long version, bool allowCensusMode)
        {
            var surveyDto = new SurveyDto(id, questionnaireDocument.Title, version, allowCensusMode);
            this.surveyDtoDocumentStorage.Store(surveyDto, surveyDto.Id);
        }

        private void RemoveSurveyDto(Guid id, long version)
        {
            this.surveyDtoDocumentStorage.Remove(SurveyDto.GetStorageId(id, version));
        }
    }
}
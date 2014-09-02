using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Utility;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Utils;

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
                                      IEventHandler<QuestionnaireDeleted>,
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
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption, QuestionType.DropDownList, QuestionType.YesNo };

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
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId, 
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
            AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
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
            var questionnaireTemplate = questionnaireStorage.GetById(questionnaireId, questionnaireVersion);
            if (questionnaireTemplate == null)
                return;

            var items = new List<FeaturedItem>();
            foreach (var featuredQuestion in questionnaireTemplate.Questionnaire.Find<IQuestion>(q => q.Featured))
            {
                var item = answeredQuestions.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);
                items.Add(CreateFeaturedItem(featuredQuestion, item == null ? null : item.Answer));
            }
            
            questionnaireDtOdocumentStorage.Store(
                new QuestionnaireDTO(interviewId, responsibleId, questionnaireId, status,
                                     items, questionnaireTemplate.Version, comments, createdOnClient, canBeDeleted), interviewId);
        }

        private FeaturedItem CreateFeaturedItem(IQuestion featuredQuestion, object answer)
        {
            if (questionTypesWithOptions.Contains(featuredQuestion.QuestionType))
            {
                var featuredCategoricalOptions = featuredQuestion.Answers.Select(
                    option =>
                        new FeaturedCategoricalOption()
                        {
                            OptionValue = decimal.Parse(option.AnswerValue),
                            OptionText = option.AnswerText,
                        });

                return new FeaturedCategoricalItem(featuredQuestion.PublicKey, 
                    featuredQuestion.QuestionText,
                    AnswerUtils.AnswerToString(Convert.ToDecimal(answer),
                        (optionValue) => getCategoricalAnswerOptionText(featuredCategoricalOptions, optionValue)),
                    featuredCategoricalOptions);
            }

            return new FeaturedItem(featuredQuestion.PublicKey, featuredQuestion.QuestionText,
                AnswerUtils.AnswerToString(answer));
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            AddOrUpdateInterviewToDashboard(evnt.Payload.InterviewData.QuestionnaireId,
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

        private void AnswerQuestion(Guid interviewId, Guid questionId, object answer)
        {
            QuestionnaireDTO questionnaire = questionnaireDtOdocumentStorage.GetById(interviewId);

            if (questionnaire == null)
                return;

            var properties = questionnaire.GetProperties();
            int keyIndex = Array.FindIndex(properties, w => w.PublicKey == questionId);

            if (keyIndex >= 0)
            {
                var featuredQuestion = properties[keyIndex];

                featuredQuestion.Value = getAnswer(featuredQuestion, answer);

                questionnaire.SetProperties(properties);
                questionnaireDtOdocumentStorage.Store(questionnaire, interviewId);
            }
        }

        private string getAnswer(FeaturedItem featuredQuestion, object answer)
        {
            var featuredCategoricalQuestion = featuredQuestion as FeaturedCategoricalItem;
            if (featuredCategoricalQuestion != null)
                return AnswerUtils.AnswerToString(answer,
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
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValue);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                new GeoPosition(latitude: evnt.Payload.Latitude, longitude: evnt.Payload.Longitude,
                    accuracy: evnt.Payload.Accuracy, altitude:evnt.Payload.Altitude, 
                    timestamp: evnt.Payload.Timestamp));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, string.Empty);
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
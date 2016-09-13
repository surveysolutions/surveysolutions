using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerQuestionnaireAccessor : IInterviewerQuestionnaireAccessor
    {
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IInterviewerInterviewAccessor interviewFactory;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments;
        private readonly IOptionsRepository optionsRepository;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;

        public InterviewerQuestionnaireAccessor(
            IJsonAllTypesSerializer synchronizationSerializer,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IInterviewerInterviewAccessor interviewFactory, 
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments, 
            IOptionsRepository optionsRepository, 
            IPlainStorage<TranslationInstance> translationsStorage)
        {
            this.synchronizationSerializer = synchronizationSerializer;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.interviewFactory = interviewFactory;
            this.questionnaireDocuments = questionnaireDocuments;
            this.optionsRepository = optionsRepository;
            this.translationsStorage = translationsStorage;
        }

        public void StoreQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census, List<TranslationDto> translationDtos)
        {
            var serializedQuestionnaireDocument = this.synchronizationSerializer.Deserialize<QuestionnaireDocument>(questionnaireDocument);
            serializedQuestionnaireDocument.ParseCategoricalQuestionOptions();

            optionsRepository.RemoveOptionsForQuestionnaire(questionnaireIdentity);

            var questionsWithLongOptionsList = serializedQuestionnaireDocument.Find<SingleQuestion>(
                x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

            foreach (var question in questionsWithLongOptionsList)
            {
                var questionTranslations = translationDtos.Where(x => x.QuestionnaireEntityId == question.PublicKey).ToList();

                this.optionsRepository.StoreOptionsForQuestion(questionnaireIdentity, question.PublicKey, question.Answers, questionTranslations);

                //remove original answers after saving
                //to save resources
                question.Answers = new List<Answer>();
            }

            var questionsWithLongOptionsIds = questionsWithLongOptionsList.Select(x => x.PublicKey).ToList();

            List<TranslationInstance> filteredTranslations = translationDtos
                .Except(x => questionsWithLongOptionsIds.Contains(x.QuestionnaireEntityId) && x.Type == TranslationType.OptionTitle)
                .Select(translationDto => new TranslationInstance
                {
                    QuestionnaireId = questionnaireIdentity.ToString(),
                    TranslationId = translationDto.TranslationId,
                    QuestionnaireEntityId = translationDto.QuestionnaireEntityId,
                    Type = translationDto.Type,
                    TranslationIndex = translationDto.TranslationIndex,
                    Value = translationDto.Value,
                    Id = Guid.NewGuid().FormatGuid()
                }).ToList();

            this.StoreTranslations(questionnaireIdentity, filteredTranslations);

            this.questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                serializedQuestionnaireDocument);

            this.questionnaireViewRepository.Store(new QuestionnaireView
            {
                Id = questionnaireIdentity.ToString(),
                Identity = questionnaireIdentity,
                Census = census,
                Title = serializedQuestionnaireDocument.Title
            });
        }

        public async Task RemoveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireId = questionnaireIdentity.ToString();

            this.DeleteInterviewsByQuestionnaire(questionnaireId);
            this.questionnaireStorage.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            this.questionnaireViewRepository.Remove(questionnaireId);
            await this.questionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);
            optionsRepository.RemoveOptionsForQuestionnaire(questionnaireIdentity);
            this.RemoveTranslations(questionnaireIdentity);
        }

        private void DeleteInterviewsByQuestionnaire(string questionnaireId)
        {
            var interviewIdsByQuestionnaire = this.interviewViewRepository
                .Where(interview => interview.QuestionnaireId == questionnaireId)
                .Select(interview => interview.InterviewId);

            foreach (var interviewId in interviewIdsByQuestionnaire)
            {
                this.interviewFactory.RemoveInterview(interviewId);
            }
        }

        public async Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            await this.questionnaireAssemblyFileAccessor.StoreAssemblyAsync(questionnaireIdentity, assembly);
        }

        public void StoreTranslations(QuestionnaireIdentity questionnaireIdentity, List<TranslationInstance> translationInstances)
        {
            this.RemoveTranslations(questionnaireIdentity);
            this.translationsStorage.Store(translationInstances);
        }

        private void RemoveTranslations(QuestionnaireIdentity questionnaireIdentity)
        {
            string questionnaireId = questionnaireIdentity.ToString();

            var storedTranslations =
                this.translationsStorage.Where(x => x.QuestionnaireId == questionnaireId);

            this.translationsStorage.Remove(storedTranslations);
        }

        public List<QuestionnaireIdentity> GetCensusQuestionnaireIdentities()
        {
            return this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census)
                    .Select(questionnaire => questionnaire.Identity)
                    .ToList();
        }

        public List<QuestionnaireIdentity> GetAllQuestionnaireIdentities()
        {
            return this.questionnaireViewRepository.Where(x => true)
                 .Select(questionnaire => questionnaire.Identity)
                 .ToList();
        }

        public bool IsAttachmentUsedAsync(string contentId)
        {
            var questionnaireDocumentViews = this.questionnaireDocuments.LoadAll().Where(x => !x.Document.IsDeleted).ToList();
            return questionnaireDocumentViews.Count > 0 && questionnaireDocumentViews.Any(x => x.Document.Attachments.Any(a => a.ContentId == contentId));
        }

        public bool IsQuestionnaireExists(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.questionnaireViewRepository.GetById(questionnaireIdentity.ToString()) != null;
        }

        public bool IsQuestionnaireAssemblyExists(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(
                questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
        }
    }
}
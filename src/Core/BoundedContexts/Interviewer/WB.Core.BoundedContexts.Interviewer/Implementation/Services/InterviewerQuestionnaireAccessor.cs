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

        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocuments;
        private readonly IOptionsRepository optionsRepository;
        private readonly IAsyncPlainStorage<TranslationInstance> translationsStorage;

        public InterviewerQuestionnaireAccessor(
            IJsonAllTypesSerializer synchronizationSerializer,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IInterviewerInterviewAccessor interviewFactory, 
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocuments, 
            IOptionsRepository optionsRepository, 
            IAsyncPlainStorage<TranslationInstance> translationsStorage)
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

        public async Task StoreQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census, List<TranslationDto> translationDtos)
        {
            var serializedQuestionnaireDocument = this.synchronizationSerializer.Deserialize<QuestionnaireDocument>(questionnaireDocument);
            serializedQuestionnaireDocument.ParseCategoricalQuestionOptions();
            
            await optionsRepository.RemoveOptionsForQuestionnaireAsync(questionnaireIdentity);

            var questionsWithLongOptionsList = serializedQuestionnaireDocument.Find<SingleQuestion>(
                x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

            foreach (var question in questionsWithLongOptionsList)
            {
                var questionTranslations = translationDtos.Where(x => x.QuestionnaireEntityId == question.PublicKey).ToList();

                await this.optionsRepository.StoreOptionsForQuestionAsync(questionnaireIdentity, question.PublicKey, question.Answers, questionTranslations);

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

            await this.StoreTranslationsAsync(questionnaireIdentity, filteredTranslations);

            this.questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                serializedQuestionnaireDocument);

            await this.questionnaireViewRepository.StoreAsync(new QuestionnaireView
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

            await this.DeleteInterviewsByQuestionnaireAsync(questionnaireId);
            this.questionnaireStorage.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            await this.questionnaireViewRepository.RemoveAsync(questionnaireId);
            await this.questionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);
            await optionsRepository.RemoveOptionsForQuestionnaireAsync(questionnaireIdentity);
            await this.RemoveTranslationsAsync(questionnaireIdentity).ConfigureAwait(false);
        }

        private async Task DeleteInterviewsByQuestionnaireAsync(string questionnaireId)
        {
            var interviewIdsByQuestionnaire = this.interviewViewRepository
                .Where(interview => interview.QuestionnaireId == questionnaireId)
                .Select(interview => interview.InterviewId);

            foreach (var interviewId in interviewIdsByQuestionnaire)
            {
                await this.interviewFactory.RemoveInterviewAsync(interviewId);
            }
        }

        public async Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            await this.questionnaireAssemblyFileAccessor.StoreAssemblyAsync(questionnaireIdentity, assembly);
        }

        public async Task StoreTranslationsAsync(QuestionnaireIdentity questionnaireIdentity, List<TranslationInstance> translationInstances)
        {
            await this.RemoveTranslationsAsync(questionnaireIdentity);
            await this.translationsStorage.StoreAsync(translationInstances);
        }

        private async Task RemoveTranslationsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            string questionnaireId = questionnaireIdentity.ToString();

            var storedTranslations =
                await this.translationsStorage.WhereAsync(x => x.QuestionnaireId == questionnaireId).ConfigureAwait(false);

            await this.translationsStorage.RemoveAsync(storedTranslations).ConfigureAwait(false);
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
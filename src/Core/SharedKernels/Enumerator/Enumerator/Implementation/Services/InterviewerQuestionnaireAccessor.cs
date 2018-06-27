using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InterviewerQuestionnaireAccessor : IInterviewerQuestionnaireAccessor
    {
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments;
        private readonly IOptionsRepository optionsRepository;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;

        public InterviewerQuestionnaireAccessor(
            IJsonAllTypesSerializer synchronizationSerializer,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor,
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments, 
            IOptionsRepository optionsRepository, 
            IPlainStorage<TranslationInstance> translationsStorage)
        {
            this.synchronizationSerializer = synchronizationSerializer;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
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
                Census = census,
                Title = serializedQuestionnaireDocument.Title
            });
        }

        public void RemoveQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireId = questionnaireIdentity.ToString();
            
            this.questionnaireStorage.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            this.questionnaireViewRepository.Remove(questionnaireId);
            this.questionnaireAssemblyFileAccessor.RemoveAssembly(questionnaireIdentity);
            optionsRepository.RemoveOptionsForQuestionnaire(questionnaireIdentity);
            this.RemoveTranslations(questionnaireIdentity);
        }

        public async Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            await this.questionnaireAssemblyFileAccessor.StoreAssemblyAsync(questionnaireIdentity, assembly);
        }

        public QuestionnaireDocument GetQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
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
                    .Select(questionnaire => QuestionnaireIdentity.Parse(questionnaire.Id))
                    .ToList();
        }

        public List<QuestionnaireIdentity> GetAllQuestionnaireIdentities()
        {
            return this.questionnaireViewRepository.Where(x => true)
                 .Select(questionnaire => QuestionnaireIdentity.Parse(questionnaire.Id))
                 .ToList();
        }

        public IReadOnlyCollection<QuestionnaireDocumentView> LoadAll()
        {
            return this.questionnaireDocuments.LoadAll();
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

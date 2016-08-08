using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IOptionsRepository optionsRepository;
        private readonly IAsyncPlainStorage<TranslationInstance> translationsStorage;


        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, 
            IOptionsRepository optionsRepository,
            IAsyncPlainStorage<TranslationInstance> translationsStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.optionsRepository = optionsRepository;
            this.translationsStorage = translationsStorage;
        }

        public async Task ImportQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument, string supportingAssembly, TranslationDto[] translations)
        {
            await this.optionsRepository.RemoveOptionsForQuestionnaireAsync(questionnaireIdentity);

            var questionsWithLongOptionsList = questionnaireDocument.Find<SingleQuestion>(
                x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

            foreach (var question in questionsWithLongOptionsList)
            {
                var questionTranslations = translations.Where(x => x.QuestionnaireEntityId == question.PublicKey).ToList();

                await this.optionsRepository.StoreOptionsForQuestionAsync(questionnaireIdentity, question.PublicKey, question.Answers, questionTranslations);

                //remove original answers after saving
                //to save resources
                question.Answers = new List<Answer>();
            }

            var questionsWithLongOptionsIds = questionsWithLongOptionsList.Select(x => x.PublicKey).ToList();

            List<TranslationInstance> filteredTranslations = translations
                .Where(x => !questionsWithLongOptionsIds.Contains(x.QuestionnaireEntityId))
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

            await this.translationsStorage.RemoveAllAsync();
            await this.translationsStorage.StoreAsync(filteredTranslations);
            
            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);

            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }
}

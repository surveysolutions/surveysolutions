﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly IOptionsRepository optionsRepository;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor, 
            IOptionsRepository optionsRepository,
            IPlainStorage<TranslationInstance> translationsStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.optionsRepository = optionsRepository;
            this.translationsStorage = translationsStorage;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations)
        {
            this.optionsRepository.RemoveOptionsForQuestionnaire(questionnaireIdentity);

            var questionsWithLongOptionsList = questionnaireDocument.Find<SingleQuestion>(
                x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

            foreach (var question in questionsWithLongOptionsList)
            {
                var questionTranslations = translations.Where(x => x.QuestionnaireEntityId == question.PublicKey).ToList();

                this.optionsRepository.StoreOptionsForQuestion(questionnaireIdentity, question.PublicKey, question.Answers, questionTranslations);

                //remove original answers after saving
                //to save resources
                question.Answers = new List<Answer>();
            }

            var questionsWithLongOptionsIds = questionsWithLongOptionsList.Select(x => x.PublicKey).ToList();

            List<TranslationInstance> filteredTranslations = translations
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

            this.translationsStorage.RemoveAll();
            this.translationsStorage.Store(filteredTranslations);

            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);

            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }
}

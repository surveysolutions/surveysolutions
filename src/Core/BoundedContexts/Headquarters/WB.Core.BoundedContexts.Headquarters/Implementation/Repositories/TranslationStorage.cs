﻿using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class TranslationStorage : ITranslationStorage
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translationsRepository;

        public TranslationStorage(IPlainStorageAccessor<TranslationInstance> translationsRepository)
        {
            this.translationsRepository = translationsRepository;
        }


        public ITranslation Get(QuestionnaireIdentity questionnaireIdentity, string language)
        {
            var translations = this.translationsRepository
                .Query(t => 
                    t.Where(translation => translation.QuestionnaireId == questionnaireIdentity && translation.Language == language)
                    .Cast<TranslationDto>()
                    .ToList()
                );

            return new QuestionnaireTranslation(translations);
        }
    }
}
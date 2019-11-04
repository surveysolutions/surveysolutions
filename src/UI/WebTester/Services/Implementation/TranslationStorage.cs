using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Services.Implementation
{
    internal class TranslationStorage : ITranslationStorage
    {
        private readonly ITranslationManagementService translations;
        
        public TranslationStorage(ITranslationManagementService translations)
        {
            this.translations = translations;
        }

        public ITranslation Get(QuestionnaireIdentity questionnaireIdentity, Guid translationId)
        {
            List<TranslationInstance> storedTranslations = this.translations.GetAll(questionnaireIdentity, translationId);
            return new QuestionnaireTranslation(storedTranslations);
        }
    }
}
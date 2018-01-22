using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Services.Implementation
{
    internal class TranslationStorage : ITranslationStorage, IDisposable
    {
        private readonly ITranslationManagementService translations;
        private readonly IDisposable eviction;

        public TranslationStorage(ITranslationManagementService translations, IEvictionObservable eviction)
        {
            this.translations = translations;
            this.eviction = eviction.Subscribe(token => this.translations.Delete(new QuestionnaireIdentity(token, 1)));
        }

        public ITranslation Get(QuestionnaireIdentity questionnaireIdentity, Guid translationId)
        {
            List<TranslationInstance> storedTranslations = this.translations.GetAll(questionnaireIdentity, translationId);
            return new QuestionnaireTranslation(storedTranslations);
        }

        public void Dispose()
        {
            eviction.Dispose();
        }
    }
}
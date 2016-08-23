using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class TranslationsStorage : ITranslationStorage
    {
        private readonly IAsyncPlainStorage<TranslationInstance> translationsRepository;

        public TranslationsStorage(IAsyncPlainStorage<TranslationInstance> translationsRepository)
        {
            this.translationsRepository = translationsRepository;
        }

        public ITranslation Get(QuestionnaireIdentity questionnaire, Guid translationId)
        {
            var questionnaireId = questionnaire.ToString();

            var translations = this.translationsRepository
                .Where(translation => translation.QuestionnaireId == questionnaireId && translation.TranslationId == translationId)
                .Cast<TranslationDto>()
                .ToList();

            return new QuestionnaireTranslation(translations);
        }
    }
}
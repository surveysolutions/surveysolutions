using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure.AppDomainSpecific
{
    public class WebTesterTranslationStorage : IWebTesterTranslationStorage
    {
        private readonly IQuestionnaireTranslator translator;
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public WebTesterTranslationStorage(IQuestionnaireTranslator translator,
            IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translator = translator;
            this.translations = translations;
        }

        public QuestionnaireDocument GetTranslated(QuestionnaireDocument questionnaire, long version, string? language, out Translation? translation)
        {
            translation = null;
            if (language == null) return questionnaire;
            
            translation = questionnaire.Translations.SingleOrDefault(t => t.Name == language);

            ITranslation? questionnaireTranslation = null;

            if (translation != null)
            {
                var translationId = translation.Id;
                questionnaireTranslation = new QuestionnaireTranslation(translations.Query(_ => 
                    _.Where(t => t.TranslationId == translationId && t.QuestionnaireId.QuestionnaireId == questionnaire.PublicKey && 
                                 t.QuestionnaireId.Version == version)));
            }

            if(questionnaireTranslation == null)
                throw new InvalidOperationException("Translation must not be null.");

            return this.translator.Translate(questionnaire, questionnaireTranslation);
        }
    }
}

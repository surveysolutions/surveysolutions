using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure.AppDomainSpecific
{
    public class WebTesterTranslationStorage : IWebTesterTranslationStorage
    {
        private readonly IQuestionnaireTranslator translator;
        private List<TranslationInstance> translationsList;

        public WebTesterTranslationStorage(IQuestionnaireTranslator translator)
        {
            this.translator = translator;
        }

        public void Store(List<TranslationInstance> translations)
        {
            this.translationsList = translations;
        }

        public QuestionnaireDocument GetTranslated(QuestionnaireDocument questionnaire, long version, string language, out Translation translation)
        {
            translation = null;
            QuestionnaireDocument result = questionnaire;

            if (language != null)
            {
                translation = questionnaire.Translations.SingleOrDefault(t => t.Name == language);

                QuestionnaireTranslation questionnaireTranslation = null;

                if (translation != null)
                {
                    var translationId = translation.Id;
                    questionnaireTranslation = new QuestionnaireTranslation(translationsList.Where(t => t.TranslationId == translationId));
                }

                result = this.translator.Translate(questionnaire, questionnaireTranslation);
            }

            return result;
        }
    }
}

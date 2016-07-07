using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Translator;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    internal class QuestionnaireTranslationTestsContext
    {
        protected static IQuestionnaireTranslation CreateQuestionnaireTranslation(List<TranslationInstance> translations)
        {
            return new QuestionnaireTranslation(translations);
        }
    }
}
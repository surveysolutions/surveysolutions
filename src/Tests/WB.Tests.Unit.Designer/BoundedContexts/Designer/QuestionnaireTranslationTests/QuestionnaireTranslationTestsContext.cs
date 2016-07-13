using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Translations;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    internal class QuestionnaireTranslationTestsContext
    {
        protected static ITranslation CreateQuestionnaireTranslation(List<TranslationInstance> translations)
        {
            return new Translation(translations);
        }
    }
}
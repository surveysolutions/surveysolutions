using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    internal class QuestionnaireTranslationTestsContext
    {
        protected static ITranslation CreateQuestionnaireTranslation(List<TranslationDto> translations)
        {
            return new QuestionnaireTranslation(translations);
        }
    }
}
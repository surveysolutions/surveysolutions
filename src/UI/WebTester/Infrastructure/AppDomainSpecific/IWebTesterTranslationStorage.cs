using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure
{
    public interface IWebTesterTranslationStorage
    {
        void Store(List<TranslationInstance> translationsList);
        QuestionnaireDocument GetTranslated(QuestionnaireDocument questionnaire, long version, string language, out Translation translation);
    }
}

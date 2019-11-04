using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure
{
    public interface IWebTesterTranslationService
    {
        void Store(List<TranslationInstance> translationsList);
        PlainQuestionnaire Translate(PlainQuestionnaire questionnaire, long version, string language);
    }
}

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Infrastructure
{
    public interface IWebTesterTranslationService
    {
        PlainQuestionnaire? Translate(PlainQuestionnaire questionnaire, long version, string? language);
    }
}

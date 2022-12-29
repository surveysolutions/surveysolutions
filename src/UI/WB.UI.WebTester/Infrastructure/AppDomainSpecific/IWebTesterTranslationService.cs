using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;

namespace WB.UI.WebTester.Infrastructure
{
    public interface IWebTesterTranslationService
    {
        PlainQuestionnaire? Translate(WebTesterPlainQuestionnaire questionnaire, long version, string? language);
    }
}

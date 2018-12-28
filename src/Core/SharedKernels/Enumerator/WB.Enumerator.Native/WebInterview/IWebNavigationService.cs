using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebNavigationService
    {
        string MakeNavigationLinks(string text, Identity entityIdentity, IQuestionnaire questionnaire,
            IStatefulInterview statefulInterview, string virtualDirectoryName);
        string ResetNavigationLinksToDefault(string text);
    }
}

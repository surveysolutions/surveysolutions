using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IStatefullInterviewSearcher
    {
        SearchResults Search(IStatefulInterview interview, FilterOption[] flags, int skip, int take);
    }

    public enum FilterOption
    {
        Flagged,
        NotFlagged,
        WithComments,
        Invalid,
        Valid,
        Answered,
        NotAnswered,
        ForSupervisor,
        ForInterviewer
    }
}
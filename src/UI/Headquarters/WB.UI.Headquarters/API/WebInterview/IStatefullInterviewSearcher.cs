using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IStatefullInterviewSearcher
    {
        SearchResults Search(IStatefulInterview interview, FilterOption[] flags, long skip, long take);
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
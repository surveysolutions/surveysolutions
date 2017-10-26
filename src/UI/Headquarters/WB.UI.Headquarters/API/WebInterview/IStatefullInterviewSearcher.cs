using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IStatefullInterviewSearcher
    {
        SearchResults Search(IStatefulInterview interview, FilteringFlags[] flags, long skip, long take);
    }

    public enum FilteringFlags
    {
        Flagged,
        NotFlagged,
        WithComments,
        Invalid,
        Valid,
        Answered,
        Unanswered,
        ForSupervisor,
        ForInterviewer
    }
}
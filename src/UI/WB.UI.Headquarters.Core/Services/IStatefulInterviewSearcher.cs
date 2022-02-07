using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IStatefulInterviewSearcher
    {
        SearchResults Search(IStatefulInterview interview, IQuestionnaire questionnaire, FilterOption[] flags, int skip, int take);

        Dictionary<FilterOption, int> GetStatistics(IStatefulInterview interview);
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

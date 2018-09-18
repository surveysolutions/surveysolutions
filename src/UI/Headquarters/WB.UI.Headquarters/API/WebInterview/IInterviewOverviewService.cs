using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IInterviewOverviewService
    {
        IEnumerable<OverviewNode> GetOverview(IStatefulInterview interview);
        OverviewItemAdditionalInfo GetOverviewItemAdditionalInfo(IStatefulInterview interview, string id);
    }
}

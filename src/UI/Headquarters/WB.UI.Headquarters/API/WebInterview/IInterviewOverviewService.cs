using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IInterviewOverviewService
    {
        IEnumerable<OverviewNode> GetOverview(IStatefulInterview interview, IQuestionnaire questionnaire,
            bool isReviewMode);
        OverviewItemAdditionalInfo GetOverviewItemAdditionalInfo(IStatefulInterview interview, string entityId, Guid currentUserId);
    }
}

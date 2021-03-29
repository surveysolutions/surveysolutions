using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebInterviewInterviewEntityFactory
    {
        Sidebar GetSidebarChildSectionsOf(string currentSectionId, IStatefulInterview interview, IQuestionnaire questionnaire, string[] sectionIds, bool isReviewMode);
        InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview, IQuestionnaire questionnaire, bool isReviewMode);
        GroupStatus CalculateSimpleStatus(InterviewTreeGroup group, bool isReviewMode, IStatefulInterview interview, IQuestionnaire questionnaire);
        void ApplyValidity(Validity validity, GroupStatus status);
        Identity GetUIParent(IStatefulInterview interview, IQuestionnaire questionnaire, Identity identity);

        InterviewEntityType GetEntityType(Identity identity, IQuestionnaire callerQuestionnaire,
            IStatefulInterview interview, bool isReviewMode);
    }
}

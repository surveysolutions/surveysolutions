using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebInterviewInterviewEntityFactory
    {
        Sidebar GetSidebarChildSectionsOf(string currentSectionId, IStatefulInterview interview, string[] sectionIds, bool isReviewMode);
        InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview, IQuestionnaire questionnaire, bool isReviewMode);
        GroupStatus CalculateSimpleStatus(InterviewTreeGroup group, bool isReviewMode);
        GroupStatus GetInterviewSimpleStatus(IStatefulInterview interview, bool isReviewMode);
        void ApplyValidity(Validity validity, InterviewTreeGroup group, bool isReviewMode);
    }
}
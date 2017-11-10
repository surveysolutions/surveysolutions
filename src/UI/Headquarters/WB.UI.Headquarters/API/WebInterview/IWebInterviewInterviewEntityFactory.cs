using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IWebInterviewInterviewEntityFactory
    {
        Sidebar GetSidebarChildSectionsOf(string sectionId, IStatefulInterview interview, string[] parentIds, bool isReviewMode);
        Comment[] GetComments(InterviewTreeQuestion question, IStatefulInterview statefulInterview);
        InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview, IQuestionnaire questionnaire, bool isReviewMode);
        GroupStatus CalculateSimpleStatus(InterviewTreeGroup group, bool isReviewMode);
        GroupStatus GetInterviewSimpleStatus(IStatefulInterview interview, bool isReviewMode);
        void ApplyValidity(Validity validity, InterviewTreeGroup group, bool isReviewMode);
    }
}
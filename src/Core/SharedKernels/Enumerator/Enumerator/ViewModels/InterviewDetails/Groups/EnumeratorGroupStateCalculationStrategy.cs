using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class EnumeratorGroupStateCalculationStrategy : IGroupStateCalculationStrategy
    {
        public GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview)
        {
            var questionsCount = interview.CountEnabledQuestions(groupIdentity);
            var answeredQuestionsCount = interview.CountEnabledAnsweredQuestions(groupIdentity);

            if (interview.HasEnabledInvalidQuestionsAndStaticTexts(groupIdentity))
                return questionsCount == answeredQuestionsCount ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

            var groupStatus = GroupStatus.Completed;

            if (interview.HasUnansweredQuestions(groupIdentity))
                groupStatus = answeredQuestionsCount > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

            var subgroupStatuses = interview.GetEnabledSubgroups(groupIdentity)
                .Select(subgroup => CalculateDetailedStatus(subgroup, interview));

            foreach (var subGroupStatus in subgroupStatuses)
            {
                switch (groupStatus)
                {
                    case GroupStatus.Completed when subGroupStatus != GroupStatus.Completed:
                        return GroupStatus.Started;
                    case GroupStatus.NotStarted when subGroupStatus != GroupStatus.NotStarted:
                        return GroupStatus.Started;
                }
            }

            return groupStatus;
        }
    }
}
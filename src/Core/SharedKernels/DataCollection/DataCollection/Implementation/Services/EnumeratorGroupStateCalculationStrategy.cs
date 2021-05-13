using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class EnumeratorGroupGroupStateCalculationStrategy : IEnumeratorGroupStateCalculationStrategy
    {
        public GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var questionsCount = interview.CountEnabledQuestions(groupIdentity);
            var answeredQuestionsCount = interview.CountEnabledAnsweredQuestions(groupIdentity);

            if (!interview.IsEnabled(groupIdentity))
                return GroupStatus.Disabled;
            
            if (interview.HasEnabledInvalidQuestionsAndStaticTexts(groupIdentity))
                return questionsCount == answeredQuestionsCount ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

            var groupStatus = GroupStatus.Completed;

            if (interview.HasUnansweredQuestions(groupIdentity))
                groupStatus = answeredQuestionsCount > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

            var subgroups = interview.GetEnabledSubgroupsAndRosters(groupIdentity);

            foreach (var subGroup in subgroups)
            {
                var subGroupStatus = CalculateDetailedStatus(subGroup, interview, questionnaire);

                if (questionnaire.IsFlatRoster(subGroup.Id) && (subGroupStatus == GroupStatus.StartedInvalid || subGroupStatus == GroupStatus.CompletedInvalid))
                    return GroupStatus.StartedInvalid;

                switch (groupStatus)
                {
                    case GroupStatus.Completed when subGroupStatus != GroupStatus.Completed:
                            return GroupStatus.Started;
                    case GroupStatus.NotStarted
                    when subGroupStatus != GroupStatus.NotStarted:
                            return GroupStatus.Started;
                }
            }

            return groupStatus;
        }
    }
}

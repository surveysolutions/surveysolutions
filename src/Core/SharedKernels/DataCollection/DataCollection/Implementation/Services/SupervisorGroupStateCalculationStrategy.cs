using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class SupervisorGroupStateCalculationStrategy : ISupervisorGroupStateCalculationStrategy
    {
        public GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            GroupStatus status;
            var group = interview.GetGroup(groupIdentity);

            if (group == null)
                return GroupStatus.Completed;

            if (group.CountEnabledInvalidQuestionsAndStaticTextsForSupervisor() > 0)
            {
                return GroupStatus.StartedInvalid;
            }

            var countEnabledAnsweredQuestions = group.CountEnabledAnsweredQuestionsForSupervisor();

            if (group.HasUnansweredQuestionsForSupervisor())
                status = countEnabledAnsweredQuestions > 0 ? GroupStatus.Started : GroupStatus.NotStarted;
            else
                status = GroupStatus.Completed;

            foreach (var subGroup in interview.GetEnabledSubgroupsAndRosters(groupIdentity))
            {
                var subGroupStatus = CalculateDetailedStatus(subGroup, interview, questionnaire);

                if (questionnaire.IsFlatRoster(subGroup.Id) && (subGroupStatus == GroupStatus.StartedInvalid || subGroupStatus == GroupStatus.CompletedInvalid))
                    return GroupStatus.StartedInvalid;

                switch (status)
                {
                    case GroupStatus.Completed when subGroupStatus != GroupStatus.Completed:
                        return GroupStatus.Started;
                    case GroupStatus.NotStarted when subGroupStatus != GroupStatus.NotStarted:
                        return GroupStatus.Started;
                }
            }

            return status;
        }
    }
}

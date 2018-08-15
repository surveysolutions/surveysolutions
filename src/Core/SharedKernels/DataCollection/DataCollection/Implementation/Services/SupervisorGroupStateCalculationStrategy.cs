using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class SupervisorGroupStateCalculationStrategy : ISupervisorGroupStateCalculationStrategy
    {
        public GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview)
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

            foreach (var subGroup in GetSubgroupStatuses())
            {
                switch (status)
                {
                    case GroupStatus.Completed when subGroup != GroupStatus.Completed:
                        return GroupStatus.Started;
                    case GroupStatus.NotStarted when subGroup != GroupStatus.NotStarted:
                        return GroupStatus.Started;
                }
            }

            return status;

            IEnumerable<GroupStatus> GetSubgroupStatuses()
            {
                return group.Children.OfType<InterviewTreeGroup>()
                    .Where(c => c.IsDisabled() == false)
                    .Select(subgroup => this.CalculateDetailedStatus(subgroup.Identity, interview));
            }
        }
    }
}

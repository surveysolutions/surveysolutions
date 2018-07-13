using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorGroupStateCalculationStrategy : IGroupStateCalculationStrategy
    {
        public GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview)
        {
            GroupStatus status;
            var group = interview.GetGroup(groupIdentity);

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

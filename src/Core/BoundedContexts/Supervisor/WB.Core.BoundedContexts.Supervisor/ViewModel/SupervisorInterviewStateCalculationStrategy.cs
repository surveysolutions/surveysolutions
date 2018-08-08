using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorInterviewStateCalculationStrategy : IInterviewStateCalculationStrategy
    {
        public SimpleGroupStatus CalculateSimpleStatus(IStatefulInterview interview)
        {
            var invalidAnswersCount = interview.CountInvalidEntitiesInInterviewForSupervisor();
            var questionsCount = interview.CountActiveQuestionsInInterviewForSupervisor();
            var answeredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterviewForSupervisor();

            if (invalidAnswersCount > 0)
                return SimpleGroupStatus.Invalid;

            if (questionsCount == answeredQuestionsCount)
                return SimpleGroupStatus.Completed;

            return SimpleGroupStatus.Other;
        }

        public GroupStatus CalculateDetailedStatus(IStatefulInterview interview)
        {
            var questionsCount = interview.CountActiveQuestionsInInterviewForSupervisor();
            var answeredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterviewForSupervisor();
            var simpleStatus = this.CalculateSimpleStatus(interview);

            switch (simpleStatus)
            {
                case SimpleGroupStatus.Completed:
                    return GroupStatus.Completed;

                case SimpleGroupStatus.Invalid:
                    return questionsCount == answeredQuestionsCount ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

                case SimpleGroupStatus.Other:
                    return answeredQuestionsCount > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

                default:
                    return GroupStatus.Started;
            }
        }
    }
}

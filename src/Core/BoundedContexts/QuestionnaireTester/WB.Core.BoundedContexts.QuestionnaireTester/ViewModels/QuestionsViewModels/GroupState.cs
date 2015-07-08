using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupState
    {
        private readonly Identity groupIdentity;

        public GroupState(Identity groupIdentity)
        {
            this.groupIdentity = groupIdentity;
        }

        public int AnsweredQuestionsCount { get; private set; }
        public int SubgroupsCount { get; private set; }
        public int QuestionsCount { get; private set; }
        public int InvalidAnswersCount { get; private set; }
        public GroupStatus Status { get; private set; }
        public SimpleGroupStatus SimpleStatus { get; private set; }

        public void UpdateSelfFromGroupModelRecursively(IStatefulInterview interview)
        {
            this.UpdateSelfFromModel(interview);

//            this.QuestionsCount = interview.CountInterviewerQuestionsInGroupRecursively(this.groupIdentity);
//            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.groupIdentity);
//            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupRecursively(this.groupIdentity);
//            this.InvalidAnswersCount = interview.CountInvalidInterviewerAnswersInGroupRecursively(this.groupIdentity);
//
//            this.UpdateStatus();
        }

        public void UpdateSelfFromGroupModelOnly(IStatefulInterview interview)
        {
            this.UpdateSelfFromModel(interview);

//            this.QuestionsCount = interview.CountActiveInterviewerQuestionsInGroupOnly(this.groupIdentity);
//            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.groupIdentity);
//            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupOnly(this.groupIdentity);
//            this.InvalidAnswersCount = interview.CountInvalidInterviewerQuestionsInGroupOnly(this.groupIdentity);
//
//            this.UpdateStatus();
        }

        private void UpdateStatus()
        {
            this.Status = GroupStatus.NotStarted;

            if (this.AnsweredQuestionsCount > 0)
                this.Status = GroupStatus.Started;

            if (this.QuestionsCount == this.AnsweredQuestionsCount)
                this.Status = GroupStatus.Completed;

            if (this.InvalidAnswersCount > 0)
                this.Status = GroupStatus.StartedInvalid;

            if (this.InvalidAnswersCount > 0 && this.QuestionsCount == this.AnsweredQuestionsCount)
                this.Status = GroupStatus.CompletedInvalid;
        }

        public void UpdateSelfFromModel(IStatefulInterview interview)
        {
            this.QuestionsCount = interview.CountActiveInterviewerQuestionsInGroupOnly(this.groupIdentity);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.groupIdentity);
            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupOnly(this.groupIdentity);
            this.InvalidAnswersCount = interview.CountInvalidInterviewerQuestionsInGroupOnly(this.groupIdentity);

            this.SimpleStatus = CalculateSimpleStatus(this.groupIdentity, interview);

            this.Status = this.CalculateDetailedStatus();
        }

        private static SimpleGroupStatus CalculateSimpleStatus(Identity group, IStatefulInterview interview)
        {
            if (interview.HasInvalidInterviewerQuestionsInGroupOnly(group))
                return SimpleGroupStatus.Invalid;

            if (interview.HasUnansweredInterviewerQuestionsInGroupOnly(group))
                return SimpleGroupStatus.Other;

            bool isSomeSubgroupNotCompleted = interview
                .GetEnabledSubgroups(@group)
                .Select(subgroup => CalculateSimpleStatus(subgroup, interview))
                .Any(status => status != SimpleGroupStatus.Completed);

            if (isSomeSubgroupNotCompleted)
                return SimpleGroupStatus.Other;

            return SimpleGroupStatus.Completed;
        }

        private GroupStatus CalculateDetailedStatus()
        {
            switch (this.SimpleStatus)
            {
                case SimpleGroupStatus.Completed:
                    return GroupStatus.Completed;

                case SimpleGroupStatus.Invalid:
                    return this.AreAllQuestionsAnswered() ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

                case SimpleGroupStatus.Other:
                    return this.IsStarted() ? GroupStatus.Started : GroupStatus.NotStarted;

                default:
                    return GroupStatus.Started;
            }
        }

        private bool IsStarted()
        {
            return this.AnsweredQuestionsCount > 0;
        }

        private bool AreAllQuestionsAnswered()
        {
            return this.QuestionsCount == this.AnsweredQuestionsCount;
        }
    }
}
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public struct GroupState
    {
        private readonly Identity groupIdentity;

        public GroupState(Identity groupIdentity)
            : this()
        {
            this.groupIdentity = groupIdentity;
        }

        public int AnsweredQuestionsCount { get; private set; }

        public int SubgroupsCount { get; private set; }

        public int QuestionsCount { get; private set; }

        public int InvalidAnswersCount { get; private set; }

        public GroupStatus Status { get; private set; }

        public void UpdateSelfFromGroupModelRecursively(IStatefulInterview interview)
        {
            this.QuestionsCount = interview.CountInterviewerQuestionsInGroupRecursively(this.groupIdentity);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.groupIdentity);
            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupRecursively(this.groupIdentity);
            this.InvalidAnswersCount = interview.CountInvalidInterviewerAnswersInGroupRecursively(this.groupIdentity);

            this.UpdateStatus();
        }

        public void UpdateSelfFromGroupModelOnly(IStatefulInterview interview)
        {
            this.QuestionsCount = interview.CountActiveInterviewerQuestionsInGroupOnly(this.groupIdentity);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.groupIdentity);
            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupOnly(this.groupIdentity);
            this.InvalidAnswersCount = interview.CountInvalidInterviewerAnswersInGroupOnly(this.groupIdentity);

            this.UpdateStatus();
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
    }
}
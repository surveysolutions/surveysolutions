using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class GroupStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        protected GroupStateViewModel()
        {
        }

        public GroupStateViewModel(IStatefulInterviewRepository interviewRepository)
        {
            this.interviewRepository = interviewRepository;
        }

        private string interviewId;

        private Identity group;

        private ScreenType screenType;

        public virtual void Init(string interviewId, Identity groupIdentity, ScreenType screenType = ScreenType.Group)
        {
            this.interviewId = interviewId;
            this.group = groupIdentity;
            this.screenType = screenType;
            this.UpdateFromGroupModel();
        }

        public int AnsweredQuestionsCount { get; private set; }
        public int SubgroupsCount { get; private set; }
        public int QuestionsCount { get; private set; }
        public int InvalidAnswersCount { get; private set; }

        private GroupStatus status;
        public GroupStatus Status
        {
            get { return this.status; }
            private set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        private SimpleGroupStatus simpleStatus;
        public SimpleGroupStatus SimpleStatus
        {
            get { return this.simpleStatus; }
            private set { this.RaiseAndSetIfChanged(ref this.simpleStatus, value); }
        }

        public void UpdateFromGroupModel()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);

            if (this.screenType != ScreenType.Group)
            {
                this.QuestionsCount = interview.CountActiveQuestionsInInterview();
                this.SubgroupsCount = 0;
                this.AnsweredQuestionsCount = interview.CountAnsweredQuestionsInInterview();
                this.InvalidAnswersCount = interview.CountInvalidQuestionsInInterview();

                this.SimpleStatus = CalculateInterviewSimpleStatus(interview);
            }
            else
            {
                this.QuestionsCount = interview.CountActiveInterviewerQuestionsInGroupOnly(this.group);
                this.SubgroupsCount = interview.GetGroupsInGroupCount(this.group);
                this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupOnly(this.group);
                this.InvalidAnswersCount = interview.CountInvalidInterviewerQuestionsInGroupOnly(this.group);

                this.SimpleStatus = CalculateSimpleStatus(this.group, interview);
            }

            this.Status = this.CalculateDetailedStatus();
        }

        private SimpleGroupStatus CalculateInterviewSimpleStatus(IStatefulInterview interview)
        {
            if (InvalidAnswersCount > 0)
                return SimpleGroupStatus.Invalid;

            if (AreAllQuestionsAnswered())
                return SimpleGroupStatus.Completed;

            return SimpleGroupStatus.Other;
        }

        private static SimpleGroupStatus CalculateSimpleStatus(Identity group, IStatefulInterview interview)
        {
            if (interview.HasInvalidInterviewerQuestionsInGroupOnly(group))
                return SimpleGroupStatus.Invalid;

            if (interview.HasUnansweredInterviewerQuestionsInGroupOnly(group))
                return SimpleGroupStatus.Other;

            bool isSomeSubgroupNotCompleted = interview
                .GetEnabledSubgroups(group)
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
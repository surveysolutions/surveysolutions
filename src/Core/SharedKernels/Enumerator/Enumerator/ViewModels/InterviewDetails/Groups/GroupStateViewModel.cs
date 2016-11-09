using System.Linq;
using MvvmCross.Core.ViewModels;
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

        public virtual void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.group = groupIdentity;
            this.UpdateFromGroupModel();
        }

        public int AnsweredQuestionsCount { get; protected set; }
        public int SubgroupsCount { get; protected set; }
        public int QuestionsCount { get; protected set; }
        public int InvalidAnswersCount { get; protected set; }

        private GroupStatus status;
        public GroupStatus Status
        {
            get { return this.status; }
            protected set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        private SimpleGroupStatus simpleStatus;
        public SimpleGroupStatus SimpleStatus
        {
            get { return this.simpleStatus; }
            protected set { this.RaiseAndSetIfChanged(ref this.simpleStatus, value); }
        }

        public virtual void UpdateFromGroupModel()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);
         
            this.QuestionsCount = interview.CountEnabledQuestions(this.group);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.group);
            this.AnsweredQuestionsCount = interview.CountEnabledAnsweredQuestions(this.group);
            this.InvalidAnswersCount = interview.CountEnabledInvalidQuestionsAndStaticTexts(this.group);

            this.SimpleStatus = CalculateSimpleStatus(this.group, interview);

            this.Status = this.CalculateDetailedStatus();
        }

        private static SimpleGroupStatus CalculateSimpleStatus(Identity group, IStatefulInterview interview)
        {
            if (interview.HasEnabledInvalidQuestionsAndStaticTexts(group))
                return SimpleGroupStatus.Invalid;

            if (interview.HasUnansweredQuestions(group))
                return SimpleGroupStatus.Other;

            bool isSomeSubgroupNotCompleted = interview
                .GetEnabledSubgroups(group)
                .Select(subgroup => CalculateSimpleStatus(subgroup, interview))
                .Any(status => status != SimpleGroupStatus.Completed);

            if (isSomeSubgroupNotCompleted)
                return SimpleGroupStatus.Other;

            return SimpleGroupStatus.Completed;
        }

        protected GroupStatus CalculateDetailedStatus()
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

        protected bool AreAllQuestionsAnswered()
        {
            return this.QuestionsCount == this.AnsweredQuestionsCount;
        }
    }
}
using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Groups
{
    public class GroupStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        public GroupStateViewModel(IStatefulInterviewRepository interviewRepository)
        {
            this.interviewRepository = interviewRepository;
        }

        private string interviewId;
        private Identity group;

        public void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.group = groupIdentity;

            this.UpdateFromModel();
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

        public void UpdateFromModel()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);

            this.QuestionsCount = interview.CountActiveInterviewerQuestionsInGroupOnly(this.group);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.group);
            this.AnsweredQuestionsCount = interview.CountAnsweredInterviewerQuestionsInGroupOnly(this.group);
            this.InvalidAnswersCount = interview.CountInvalidInterviewerQuestionsInGroupOnly(this.group);

            this.SimpleStatus = CalculateSimpleStatus(this.group, interview);

            this.Status = this.CalculateDetailedStatus();
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
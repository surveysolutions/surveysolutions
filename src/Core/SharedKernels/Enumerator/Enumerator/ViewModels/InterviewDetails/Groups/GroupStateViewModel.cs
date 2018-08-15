using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class GroupStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IGroupStateCalculationStrategy groupStateCalculationStrategy;

        protected GroupStateViewModel()
        {
        }

        public GroupStateViewModel(IStatefulInterviewRepository interviewRepository,
            IGroupStateCalculationStrategy groupStateCalculationStrategy)
        {
            this.interviewRepository = interviewRepository;
            this.groupStateCalculationStrategy = groupStateCalculationStrategy;
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
            get => this.status;
            protected set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        private SimpleGroupStatus simpleStatus;
        public SimpleGroupStatus SimpleStatus
        {
            get => this.simpleStatus;
            protected set => this.RaiseAndSetIfChanged(ref this.simpleStatus, value);
        }

        public virtual void UpdateFromGroupModel()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);
            this.QuestionsCount = interview.CountEnabledQuestions(this.group);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.group);
            this.AnsweredQuestionsCount = interview.CountEnabledAnsweredQuestions(this.group);
            this.InvalidAnswersCount = interview.CountEnabledInvalidQuestionsAndStaticTexts(this.group);
            this.Status = this.CalculateDetailedStatus(this.group, interview);
            this.SimpleStatus = CalculateSimpleStatus();
        }

        private SimpleGroupStatus CalculateSimpleStatus()
        {
            switch (this.Status)
            {
                case GroupStatus.Completed:
                    return SimpleGroupStatus.Completed;
                case GroupStatus.StartedInvalid:
                case GroupStatus.CompletedInvalid:
                    return SimpleGroupStatus.Invalid;
                default:
                    return SimpleGroupStatus.Other;
            }
        }

        private GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview)
        {
            return this.groupStateCalculationStrategy.CalculateDetailedStatus(groupIdentity, interview);
        }
    }
}

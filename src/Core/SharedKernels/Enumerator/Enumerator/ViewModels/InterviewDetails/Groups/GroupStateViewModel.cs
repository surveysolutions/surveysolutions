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
        protected readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IQuestionnaireStorage questionnaireRepository;
        protected readonly IGroupStateCalculationStrategy groupStateCalculationStrategy;

        protected GroupStateViewModel()
        {
        }

        public GroupStateViewModel(IStatefulInterviewRepository interviewRepository,
            IGroupStateCalculationStrategy groupStateCalculationStrategy,
            IQuestionnaireStorage questionnaireRepository)
        {
            this.interviewRepository = interviewRepository;
            this.groupStateCalculationStrategy = groupStateCalculationStrategy;
            this.questionnaireRepository = questionnaireRepository;
        }

        protected string interviewId;

        protected Identity group;

        public virtual void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.group = groupIdentity;
            this.UpdateFromGroupModel();
        }

        private int answeredQuestionsCount; 
        public int AnsweredQuestionsCount 
        {
            get => this.answeredQuestionsCount;
            protected set => this.RaiseAndSetIfChanged(ref this.answeredQuestionsCount, value);
        }

        private int questionsCount;
        public int QuestionsCount
        {
            get => this.questionsCount;
            protected set => this.RaiseAndSetIfChanged(ref this.questionsCount, value);
        }

        public int SubgroupsCount { get; protected set; }
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
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.QuestionsCount = interview.CountEnabledQuestions(this.group);
            this.SubgroupsCount = interview.GetGroupsInGroupCount(this.group);
            this.AnsweredQuestionsCount = interview.CountEnabledAnsweredQuestions(this.group);
            this.InvalidAnswersCount = interview.CountEnabledInvalidQuestionsAndStaticTexts(this.group);
            this.Status = this.CalculateDetailedStatus(this.group, interview, questionnaire);
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

        private GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            return this.groupStateCalculationStrategy.CalculateDetailedStatus(groupIdentity, interview, questionnaire);
        }
    }
}

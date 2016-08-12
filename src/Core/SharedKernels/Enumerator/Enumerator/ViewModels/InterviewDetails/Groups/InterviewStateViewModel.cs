using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class CoverStateViewModel : GroupStateViewModel
    {
        public override void Init(string interviewId, Identity groupIdentity)
        {
            this.QuestionsCount = 0;
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = 0;
            this.InvalidAnswersCount = 0;

            this.SimpleStatus = SimpleGroupStatus.Other;
            this.Status = GroupStatus.Started;
        }

        public override void UpdateFromGroupModel()
        {
        }
    }

    public class InterviewStateViewModel : GroupStateViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        protected InterviewStateViewModel()
        {
        }

        public InterviewStateViewModel(IStatefulInterviewRepository interviewRepository)
        {
            this.interviewRepository = interviewRepository;
        }

        private string interviewId;

        public override void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.UpdateFromGroupModel();
        }

        public override void UpdateFromGroupModel()
        {           
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);

            this.QuestionsCount = interview.CountActiveQuestionsInInterview();
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = interview.CountAnsweredQuestionsInInterview();
            this.InvalidAnswersCount = interview.CountInvalidEntitiesInInterview();

            this.SimpleStatus = this.CalculateInterviewSimpleStatus();
            this.Status = this.CalculateDetailedStatus();
        }

        private SimpleGroupStatus CalculateInterviewSimpleStatus()
        {
            if (InvalidAnswersCount > 0)
                return SimpleGroupStatus.Invalid;

            if (AreAllQuestionsAnswered())
                return SimpleGroupStatus.Completed;

            return SimpleGroupStatus.Other;
        }
    }
}
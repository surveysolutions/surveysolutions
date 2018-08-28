using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class InterviewStateViewModel : GroupStateViewModel
    {
        private readonly IInterviewStateCalculationStrategy interviewStateCalculationStrategy;
        private readonly IStatefulInterviewRepository interviewRepository;

        protected InterviewStateViewModel()
        {
        }

        public InterviewStateViewModel(IStatefulInterviewRepository interviewRepository, IInterviewStateCalculationStrategy interviewStateCalculationStrategy)
        {
            this.interviewStateCalculationStrategy = interviewStateCalculationStrategy;
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
            this.AnsweredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterview();
            this.InvalidAnswersCount = interview.CountInvalidEntitiesInInterview();
            this.SimpleStatus = this.CalculateInterviewSimpleStatus();
            this.Status = this.CalculateDetailedStatus();
        }

        private SimpleGroupStatus CalculateInterviewSimpleStatus()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);
            return this.interviewStateCalculationStrategy.CalculateSimpleStatus(interview);
        }

        private GroupStatus CalculateDetailedStatus()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);
            return this.interviewStateCalculationStrategy.CalculateDetailedStatus(interview);
        }
    }
}

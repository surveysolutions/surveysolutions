using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
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
            this.AnsweredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterview();
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

        private GroupStatus CalculateDetailedStatus()
        {
            switch (this.SimpleStatus)
            {
                case SimpleGroupStatus.Completed:
                    return GroupStatus.Completed;

                case SimpleGroupStatus.Invalid:
                    return this.AreAllQuestionsAnswered() ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

                case SimpleGroupStatus.Other:
                    return this.AnsweredQuestionsCount > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

                default:
                    return GroupStatus.Started;
            }
        }

        protected bool AreAllQuestionsAnswered() => this.QuestionsCount == this.AnsweredQuestionsCount;
    }
}

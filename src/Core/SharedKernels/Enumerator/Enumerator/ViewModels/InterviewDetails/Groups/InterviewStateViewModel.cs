using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

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

        public override void Init(string interviewId, Identity groupIdentity, ScreenType screenType = ScreenType.Group)
        {
            this.interviewId = interviewId;
            this.UpdateFromGroupModel();
        }

        public new void UpdateFromGroupModel()
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);

            this.QuestionsCount = interview.CountActiveQuestionsInInterview();
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = interview.CountAnsweredQuestionsInInterview();
            this.InvalidAnswersCount = interview.CountInvalidQuestionsInInterview();

            this.SimpleStatus = CalculateInterviewSimpleStatus(interview);
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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class InterviewStateViewModel : GroupStateViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInterfaceStateService userInterfaceStateService;

        protected InterviewStateViewModel()
        {
        }

        public InterviewStateViewModel(IStatefulInterviewRepository interviewRepository,
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.interviewRepository = interviewRepository;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        private string interviewId;

        public override void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.UpdateFromGroupModel();
        }

        public override void UpdateFromGroupModel()
        {           
            try
            {
                userInterfaceStateService.NotifyRefreshStarted();

                IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);

                this.QuestionsCount = interview.CountActiveQuestionsInInterview();
                this.SubgroupsCount = 0;
                this.AnsweredQuestionsCount = interview.CountAnsweredQuestionsInInterview();
                this.InvalidAnswersCount = interview.CountInvalidQuestionsInInterview();

                this.SimpleStatus = this.CalculateInterviewSimpleStatus();
                this.Status = this.CalculateDetailedStatus();
            }
            finally
            {
                userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        private SimpleGroupStatus CalculateInterviewSimpleStatus()
        {
            if (InvalidAnswersCount > 0)
                return SimpleGroupStatus.Invalid;

            if (AreAllQuestionsAnswered())
                return SimpleGroupStatus.Completed;

            return SimpleGroupStatus.Other;
        }

        private bool AreAllQuestionsAnswered()
        {
            return this.QuestionsCount == this.AnsweredQuestionsCount;
        }
    }
}
using MvvmCross.Platform.Core;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class InterviewStateViewModel : GroupStateViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        protected InterviewStateViewModel()
        {
        }

        public InterviewStateViewModel(IStatefulInterviewRepository interviewRepository,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
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
            mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.QuestionsCount = interview.CountActiveQuestionsInInterview();
                this.SubgroupsCount = 0;
                this.AnsweredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterview();
                this.InvalidAnswersCount = interview.CountInvalidEntitiesInInterview();

                this.SimpleStatus = this.CalculateInterviewSimpleStatus();
                this.Status = this.CalculateDetailedStatus();
            });
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
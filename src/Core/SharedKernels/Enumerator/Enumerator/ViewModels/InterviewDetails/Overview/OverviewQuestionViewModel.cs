using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewQuestionViewModel : OverviewQuestion
    {
        private readonly IUserInteractionService userInteractionService;

        public OverviewQuestionViewModel(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, IUserInteractionService userInteractionService) : base(treeQuestion, interview)
        {
            this.userInteractionService = userInteractionService;
        }

        public IMvxCommand ShowErrors => new MvxCommand(() =>
        {
            foreach (var error in this.ErrorMessages)
            {
                userInteractionService.ShowToast(error);
            }
        }, () => ErrorMessages.Count > 0);
    }
}

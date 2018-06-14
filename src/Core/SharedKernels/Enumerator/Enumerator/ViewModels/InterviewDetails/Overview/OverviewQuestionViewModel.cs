using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewQuestionViewModel : OverviewQuestion
    {
        public OverviewQuestionViewModel(InterviewTreeQuestion treeQuestion) : base(treeQuestion)
        {
        }

        public IMvxCommand ShowErrors => new MvxCommand(() =>
        {
            var userService = Mvx.Resolve<IUserInteractionService>();
            foreach (var error in this.ErrorMessages)
            {
                userService.ShowToast(error);
            }
        }, () => ErrorMessages.Count > 0);
    }
}

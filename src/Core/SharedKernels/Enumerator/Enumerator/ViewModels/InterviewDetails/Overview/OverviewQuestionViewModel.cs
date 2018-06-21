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
        public OverviewQuestionViewModel(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, IUserInteractionService userInteractionService) : base(treeQuestion, interview)
        {

        }

    }
}

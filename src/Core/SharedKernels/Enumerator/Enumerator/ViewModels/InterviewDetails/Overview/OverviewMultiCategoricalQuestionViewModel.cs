using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

public class OverviewMultiCategoricalQuestionViewModel : OverviewQuestionViewModel
{
    public OverviewMultiCategoricalQuestionViewModel(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, 
        IUserInteractionService userInteractionService) 
        : base(treeQuestion, interview, userInteractionService)
    {
    }
}

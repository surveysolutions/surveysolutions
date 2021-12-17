using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewVariableViewModel: OverviewVariable
    {
        public OverviewVariableViewModel(InterviewTreeVariable treeNode,
            IStatefulInterview interview) : base(treeNode, interview)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

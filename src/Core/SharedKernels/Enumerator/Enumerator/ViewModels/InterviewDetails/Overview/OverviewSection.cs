using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewSection : OverviewNode
    {
        public OverviewSection(InterviewTreeGroup treeNode) : base(treeNode)
        {
            this.State = OverviewNodeState.Answered;
        }

        public sealed override OverviewNodeState State { get; set; }
    }
}

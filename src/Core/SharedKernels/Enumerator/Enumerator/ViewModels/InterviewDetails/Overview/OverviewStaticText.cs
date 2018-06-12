using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewStaticText : OverviewNode
    {
        public OverviewStaticText(InterviewTreeStaticText treeNode) : base(treeNode)
        {
            if (treeNode.FailedErrors.Count > 0)
            {
                this.State = OverviewNodeState.Invalid;
            }
            else
            {
                this.State = OverviewNodeState.Answered;
            }
        }

        public sealed override OverviewNodeState State { get; set; }
    }
}

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public abstract class OverviewNode
    {
        protected OverviewNode(IInterviewTreeNode treeNode)
        {
            this.Id = treeNode.Identity.ToString();
            this.Title = treeNode.Title.Text;
        }

        public string Title { get; set; }
        public string Id { get; set; }

        public abstract OverviewNodeState State { get; set; }
    }
}

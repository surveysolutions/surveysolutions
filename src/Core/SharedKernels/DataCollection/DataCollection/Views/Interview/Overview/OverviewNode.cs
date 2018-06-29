using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public abstract class OverviewNode
    {
        protected OverviewNode(IInterviewTreeNode treeNode)
        {
            this.Id = treeNode.Identity.ToString();
            this.Title = treeNode.Title.Text;
            this.IsAnswered = true;
        }

        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsAnswered { get; set; }

        public abstract OverviewNodeState State { get; set; }
    }
}

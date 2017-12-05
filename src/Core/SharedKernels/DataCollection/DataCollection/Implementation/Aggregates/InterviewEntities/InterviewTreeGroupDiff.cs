namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeGroupDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            var sourceGroupNode = sourceNode as InterviewTreeGroup;
            var changedGroupNode = changedNode as InterviewTreeGroup;

            bool IsTitleChangedImpl()
            {
                if (this.IsNodeRemoved) return false;
                if (this.IsNodeAdded && !changedGroupNode.Title.HasSubstitutions) return false;
                return sourceGroupNode?.Title.Text != changedGroupNode.Title.Text;
            }

            this.IsTitleChanged = IsTitleChangedImpl();
        }

        public bool IsTitleChanged {get; private set; }
    }
}
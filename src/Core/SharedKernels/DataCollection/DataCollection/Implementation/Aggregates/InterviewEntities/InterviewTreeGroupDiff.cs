namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeGroupDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
        }

        public new InterviewTreeGroup SourceNode => base.SourceNode as InterviewTreeGroup;
        public new InterviewTreeGroup ChangedNode => base.ChangedNode as InterviewTreeGroup;

        public bool IsTitleChanged
        {
            get
            {
                if (this.IsNodeRemoved) return false;
                if (this.IsNodeAdded && !this.ChangedNode.Title.HasSubstitutions) return false;
                return this.SourceNode?.Title.Text != this.ChangedNode.Title.Text;
            }
        }
    }
}
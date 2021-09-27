namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeGroupDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            this.IsTitleChanged = IsTitleChangedImpl(sourceNode as InterviewTreeGroup, changedNode as InterviewTreeGroup);
        }

        bool IsTitleChangedImpl(InterviewTreeGroup sourceGroupNode, InterviewTreeGroup changedGroupNode)
        {
            if (this.IsNodeRemoved) return false;
            if (this.IsNodeAdded && !changedGroupNode.Title.HasSubstitutions) return false;
            return sourceGroupNode?.Title.Text != changedGroupNode.Title.Text;
        }

        public bool IsTitleChanged {get; }
        
        public override bool DidSubstitutableChange()
        {
            return this.IsTitleChanged;
        }

        public override string ToString()
        {
            return base.ToString() + $", TitleChanged: {IsTitleChanged}";
        }
    }
}

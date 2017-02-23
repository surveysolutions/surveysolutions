namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeStaticTextDiff : InterviewTreeValidateableDiff
    {
        public InterviewTreeStaticTextDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
        }

        public new InterviewTreeStaticText SourceNode => base.SourceNode as InterviewTreeStaticText;
        public new InterviewTreeStaticText ChangedNode => base.ChangedNode as InterviewTreeStaticText;

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
namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeStaticTextDiff : InterviewTreeValidateableDiff
    {
        public InterviewTreeStaticTextDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            IsTitleChanged = IsTitleChangedImp(SourceNode, ChangedNode);
        }

        public new InterviewTreeStaticText SourceNode => base.SourceNode as InterviewTreeStaticText;
        public new InterviewTreeStaticText ChangedNode => base.ChangedNode as InterviewTreeStaticText;

        public bool IsTitleChanged { get; }
        
        public override bool DidSubstitutableChange()
        {
            return this.IsTitleChanged ||
                   this.AreValidationMessagesChanged;
        }

        public bool IsTitleChangedImp(InterviewTreeStaticText sourceNode, InterviewTreeStaticText changedNode)
        {
            if (this.IsNodeRemoved) return false;
            if (this.IsNodeAdded && !changedNode.Title.HasSubstitutions) return false;
            return sourceNode?.Title.Text != changedNode.Title.Text;
        }
    }
}

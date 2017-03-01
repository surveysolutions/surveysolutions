namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeVariableDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeVariableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
        }

        public new InterviewTreeVariable SourceNode => base.SourceNode as InterviewTreeVariable;
        public new InterviewTreeVariable ChangedNode => base.ChangedNode as InterviewTreeVariable;

        public bool IsValueChanged => this.SourceNode == null
            ? this.ChangedNode.HasValue
            : this.SourceNode.Value != this.ChangedNode.Value;
    }
}
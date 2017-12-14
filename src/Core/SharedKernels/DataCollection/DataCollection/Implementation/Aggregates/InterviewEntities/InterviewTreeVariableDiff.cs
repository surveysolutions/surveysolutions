namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeVariableDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeVariableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            IsValueChanged = IsValueChangedImp(sourceNode as InterviewTreeVariable, changedNode as InterviewTreeVariable);
        }

        public new InterviewTreeVariable SourceNode => base.SourceNode as InterviewTreeVariable;
        public new InterviewTreeVariable ChangedNode => base.ChangedNode as InterviewTreeVariable;


        public bool IsValueChanged { get; }
        public bool IsValueChangedImp(InterviewTreeVariable sourceNode, InterviewTreeVariable changedNode)
        {
            if (sourceNode?.Value == null)
                return changedNode?.HasValue ?? false;

            if (changedNode?.Value == null)
                return sourceNode.HasValue;

            return !sourceNode.Value.Equals(changedNode.Value);
        }
    }
}
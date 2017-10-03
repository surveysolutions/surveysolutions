using System;

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

        public bool IsValueChanged
        {
            get
            {
                if (this.SourceNode?.Value == null)
                    return this.ChangedNode.HasValue;

                if (this.ChangedNode?.Value == null)
                    return this.SourceNode.HasValue;

                return !this.SourceNode.Value.Equals(this.ChangedNode.Value);
            }
        }
    }
}
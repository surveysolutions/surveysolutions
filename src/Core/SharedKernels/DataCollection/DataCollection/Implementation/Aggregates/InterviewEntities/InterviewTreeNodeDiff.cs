namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeNodeDiff
    {
        public InterviewTreeNodeDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
        {
            this.SourceNode = sourceNode;
            this.ChangedNode = changedNode;
        }
        public IInterviewTreeNode SourceNode { get; }
        public IInterviewTreeNode ChangedNode { get; }
    }

    public class InterviewTreeRosterDiff : InterviewTreeGroupDiff
    {
        public new InterviewTreeRoster SourceNode => base.SourceNode as InterviewTreeRoster;
        public new InterviewTreeRoster ChangedNode => base.ChangedNode as InterviewTreeRoster;

        public InterviewTreeRosterDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }

    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeGroup SourceNode => base.SourceNode as InterviewTreeGroup;
        public new InterviewTreeGroup ChangedNode => base.ChangedNode as InterviewTreeGroup;

        public InterviewTreeGroupDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }

    public class InterviewTreeQuestionDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeQuestion SourceNode => base.SourceNode as InterviewTreeQuestion;
        public new InterviewTreeQuestion ChangedNode => base.ChangedNode as InterviewTreeQuestion;

        public InterviewTreeQuestionDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }

    public class InterviewTreeStaticTextDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeStaticText SourceNode => base.SourceNode as InterviewTreeStaticText;
        public new InterviewTreeStaticText ChangedNode => base.ChangedNode as InterviewTreeStaticText;

        public InterviewTreeStaticTextDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }

    public class InterviewTreeVariableDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeVariable SourceNode => base.SourceNode as InterviewTreeVariable;
        public new InterviewTreeVariable ChangedNode => base.SourceNode as InterviewTreeVariable;

        public InterviewTreeVariableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }
}
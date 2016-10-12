namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeNodeDiff
    {
        public IInterviewTreeNode SourceNode { get; set; }
        public IInterviewTreeNode ChangedNode { get; set; }
    }

    public class InterviewTreeRosterDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeRoster SourceNode { get; set; }
        public new InterviewTreeRoster ChangedNode { get; set; }
    }

    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeGroup SourceNode { get; set; }
        public new InterviewTreeGroup ChangedNode { get; set; }
    }

    public class InterviewTreeQuestionDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeQuestion SourceNode { get; set; }
        public new InterviewTreeQuestion ChangedNode { get; set; }
    }

    public class InterviewTreeStaticTextDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeStaticText SourceNode { get; set; }
        public new InterviewTreeStaticText ChangedNode { get; set; }
    }

    public class InterviewTreeVariableDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeVariable SourceNode { get; set; }
        public new InterviewTreeVariable ChangedNode { get; set; }
    }
}
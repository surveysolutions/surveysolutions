namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeRosterDiff : InterviewTreeGroupDiff
    {
        public InterviewTreeRosterDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
        }

        public new InterviewTreeRoster SourceNode => base.SourceNode as InterviewTreeRoster;
        public new InterviewTreeRoster ChangedNode => base.ChangedNode as InterviewTreeRoster;

        public bool IsRosterTitleChanged
            => this.ChangedNode != null && this.SourceNode?.RosterTitle != this.ChangedNode.RosterTitle;
    }
}
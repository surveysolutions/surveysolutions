namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeRosterDiff : InterviewTreeGroupDiff
    {
        public InterviewTreeRosterDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            this.IsRosterTitleChanged = changedNode != null && (sourceNode as InterviewTreeRoster)?.RosterTitle != (changedNode as InterviewTreeRoster)?.RosterTitle;
        }

        public new InterviewTreeRoster SourceNode => base.SourceNode as InterviewTreeRoster;
        public new InterviewTreeRoster ChangedNode => base.ChangedNode as InterviewTreeRoster;


        public bool IsRosterTitleChanged { get; private set; }
    }
}
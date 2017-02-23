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

        public bool IsNodeAdded => this.SourceNode == null && this.ChangedNode != null;
        public bool IsNodeRemoved => this.SourceNode != null && this.ChangedNode == null;

        public bool IsNodeDisabled
        {
            get
            {
                var newNodeIsDisabled = this.SourceNode == null && this.ChangedNode != null &&
                    this.ChangedNode.IsDisabledByOwnCondition();
                var existingNodeWasEnadledAndWasDisabled = this.SourceNode != null && this.ChangedNode != null &&
                    !this.SourceNode.IsDisabledByOwnCondition() && this.ChangedNode.IsDisabledByOwnCondition();
                return newNodeIsDisabled || existingNodeWasEnadledAndWasDisabled;
            }
        }

        public bool IsNodeEnabled
        {
            get
            {
                var nodeIsRemovedAndWasDisabled = this.ChangedNode == null && this.SourceNode != null &&
                    this.SourceNode.IsDisabledByOwnCondition();
                var existingNodeWasEnabledAndNowIsDisabled = this.SourceNode != null && this.ChangedNode != null &&
                    this.SourceNode.IsDisabledByOwnCondition() && !this.ChangedNode.IsDisabledByOwnCondition();
                return existingNodeWasEnabledAndNowIsDisabled || nodeIsRemovedAndWasDisabled;
            }
        }

        public Identity Identity => this.ChangedNode?.Identity ?? this.SourceNode?.Identity;

        public static InterviewTreeNodeDiff Create(IInterviewTreeNode source, IInterviewTreeNode changed)
        {
            if (source is InterviewTreeRoster || changed is InterviewTreeRoster)
                return new InterviewTreeRosterDiff(source, changed);
            if (source is InterviewTreeSection || changed is InterviewTreeSection)
                return new InterviewTreeGroupDiff(source, changed);
            if (source is InterviewTreeGroup || changed is InterviewTreeGroup)
                return new InterviewTreeGroupDiff(source, changed);
            if (source is InterviewTreeQuestion || changed is InterviewTreeQuestion)
                return new InterviewTreeQuestionDiff(source, changed);
            if (source is InterviewTreeStaticText || changed is InterviewTreeStaticText)
                return new InterviewTreeStaticTextDiff(source, changed);
            if (source is InterviewTreeVariable || changed is InterviewTreeVariable)
                return new InterviewTreeVariableDiff(source, changed);

            return new InterviewTreeNodeDiff(source, changed);
        }
    }
}
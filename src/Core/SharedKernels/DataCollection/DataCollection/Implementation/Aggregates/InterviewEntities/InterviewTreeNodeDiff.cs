namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeNodeDiff
    {
        public InterviewTreeNodeDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
        {
            IsNodeAdded = sourceNode == null && changedNode != null;
            IsNodeRemoved = sourceNode != null && changedNode == null;
            Identity = changedNode?.Identity ?? sourceNode?.Identity;
            IsNodeDisabled = IsNodeDisabledImpl(sourceNode, changedNode);
            IsNodeEnabled = IsNodeEnabledImpl(sourceNode, changedNode);
            SourceNode = sourceNode;
            ChangedNode = changedNode;
        }

        public IInterviewTreeNode SourceNode { get; }
        public IInterviewTreeNode ChangedNode { get; }


        bool IsNodeDisabledImpl(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
        {
            var newNodeIsDisabled = sourceNode == null && changedNode != null &&
                                    changedNode.IsDisabledByOwnCondition();
            var existingNodeWasEnadledAndWasDisabled = sourceNode != null && changedNode != null &&
                                                       !sourceNode.IsDisabledByOwnCondition() && changedNode.IsDisabledByOwnCondition();
            return newNodeIsDisabled || existingNodeWasEnadledAndWasDisabled;
        }

        bool IsNodeEnabledImpl(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
        {
            var nodeIsRemovedAndWasDisabled = changedNode == null && sourceNode != null &&
                                              sourceNode.IsDisabledByOwnCondition();
            var existingNodeWasEnabledAndNowIsDisabled = sourceNode != null && changedNode != null &&
                                                         sourceNode.IsDisabledByOwnCondition() && !changedNode.IsDisabledByOwnCondition();
            return existingNodeWasEnabledAndNowIsDisabled || nodeIsRemovedAndWasDisabled;
        }

        public bool IsNodeAdded { get; private set; }
        public bool IsNodeRemoved { get; private set; }

        public bool IsNodeDisabled { get; private set; }

        public bool IsNodeEnabled { get; private set; }

        public Identity Identity { get; private set; }

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
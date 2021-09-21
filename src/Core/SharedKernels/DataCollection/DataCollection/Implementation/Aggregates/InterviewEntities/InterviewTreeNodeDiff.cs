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
            IsNodeEnabled = IsNodeEnabledImpl(sourceNode, changedNode) && ! IsNodeRemoved;
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
            var nodeIsRemovedAndWasDisabled = changedNode == null && sourceNode != null && sourceNode.IsDisabledByOwnCondition();
            var existingNodeWasEnabledAndNowIsDisabled = sourceNode != null && changedNode != null &&
                                                         sourceNode.IsDisabledByOwnCondition() && !changedNode.IsDisabledByOwnCondition();
            return existingNodeWasEnabledAndNowIsDisabled || nodeIsRemovedAndWasDisabled;
        }

        public virtual bool DidSubstitutableChange() => false;

        public bool IsNodeAdded { get; }
        public bool IsNodeRemoved { get; }

        public bool IsNodeDisabled { get; }

        public bool IsNodeEnabled { get; }

        public Identity Identity { get; }

        public static InterviewTreeNodeDiff Create(InterviewTreeQuestion source, InterviewTreeQuestion changed)
        {
            return new InterviewTreeQuestionDiff(source, changed);
        }
        
        public static InterviewTreeNodeDiff Create(InterviewTreeStaticText source, InterviewTreeStaticText changed)
        {
            return new InterviewTreeStaticTextDiff(source, changed);
        }
        
        public static InterviewTreeNodeDiff Create(InterviewTreeRoster source, InterviewTreeRoster changed)
        {
            return new InterviewTreeRosterDiff(source, changed);
        }
        
        public static InterviewTreeNodeDiff Create(InterviewTreeSection source, InterviewTreeSection changed)
        {
            return new InterviewTreeGroupDiff(source, changed);
        }
        
        public static InterviewTreeNodeDiff Create(InterviewTreeGroup source, InterviewTreeGroup changed)
        {
            return new InterviewTreeGroupDiff(source, changed);
        }

        public static InterviewTreeNodeDiff Create(InterviewTreeVariable source, InterviewTreeVariable changed)
        {
            return new InterviewTreeVariableDiff(source, changed);
        }

        public static InterviewTreeNodeDiff Create(IInterviewTreeNode source, IInterviewTreeNode changed)
        {
            if (source is InterviewTreeQuestion || changed is InterviewTreeQuestion)
                return new InterviewTreeQuestionDiff(source, changed);
            if (source is InterviewTreeStaticText || changed is InterviewTreeStaticText)
                return new InterviewTreeStaticTextDiff(source, changed);
            if (source is InterviewTreeRoster || changed is InterviewTreeRoster)
                return new InterviewTreeRosterDiff(source, changed);
            if (source is InterviewTreeSection || changed is InterviewTreeSection)
                return new InterviewTreeGroupDiff(source, changed);
            if (source is InterviewTreeGroup || changed is InterviewTreeGroup)
                return new InterviewTreeGroupDiff(source, changed);
            if (source is InterviewTreeVariable || changed is InterviewTreeVariable)
                return new InterviewTreeVariableDiff(source, changed);

            return new InterviewTreeNodeDiff(source, changed);
        }

        public override string ToString()
        {
            return $"{Identity}. {ChangedNode.Title} Added: {IsNodeAdded}, Removed: {IsNodeRemoved}, Disabled: {IsNodeDisabled}, Enabled: {IsNodeEnabled}";
        }
    }
}

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeNodeDiff
    {
        public IInterviewTreeNode SourceNode { get; set; }
        public IInterviewTreeNode ChangedNode { get; set; }
    }
}
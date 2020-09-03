using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewSection : OverviewNode
    {
        private OverviewSection() { }
        public OverviewSection(InterviewTreeGroup treeNode) : base(treeNode)
        {
            this.State = OverviewNodeState.Answered;
        }

        public sealed override OverviewNodeState State { get; set; }

        public static OverviewSection Empty(string title)
        {
            return new OverviewSection()
            {
                Title = title
            };
        }
    }
}

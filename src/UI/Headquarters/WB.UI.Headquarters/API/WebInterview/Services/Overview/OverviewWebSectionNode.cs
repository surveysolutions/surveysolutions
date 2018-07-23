using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebSectionNode : OverviewSection
    {
        public OverviewWebSectionNode(InterviewTreeGroup treeNode) : base(treeNode)
        {
        }

        public bool IsSection { get; } = true;
    }
}

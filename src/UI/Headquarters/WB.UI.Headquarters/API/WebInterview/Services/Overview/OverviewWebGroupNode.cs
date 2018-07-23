using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebGroupNode : OverviewGroup
    {
        public OverviewWebGroupNode(InterviewTreeGroup treeNode) : base(treeNode)
        {
            Breadcrumbs = treeNode.GetBreadcrumbs().ToList();
        }

        public bool IsGroup { get; } = true;

        public List<Link> Breadcrumbs { get; }

        public string RosterTitle { get; set; }
    }
}

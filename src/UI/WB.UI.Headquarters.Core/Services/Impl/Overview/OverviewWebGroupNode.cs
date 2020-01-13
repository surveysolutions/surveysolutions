using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebGroupNode : OverviewGroup
    {
        public OverviewWebGroupNode(InterviewTreeGroup treeNode, IQuestionnaire questionnaire) : base(treeNode)
        {
            Breadcrumbs = treeNode.GetBreadcrumbs(questionnaire).ToList();
        }

        public bool IsGroup { get; } = true;

        public List<Link> Breadcrumbs { get; }

        public string RosterTitle { get; set; }
    }
}

using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebVariableNode: OverviewVariable
    {
        public OverviewWebVariableNode(InterviewTreeVariable treeNode, IStatefulInterview interview) : base(treeNode, interview)
        {
        }
        
        public string ControlType { get; set; } = "variable";
    }
}
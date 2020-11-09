using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewVariable: OverviewNode
    {
        public OverviewVariable(InterviewTreeVariable treeNode, IStatefulInterview interview) : base(treeNode)
        {
            this.State = treeNode.HasValue ? OverviewNodeState.Answered : OverviewNodeState.Unanswered;
            
            this.Value = treeNode.GetValueAsString();
        }

        public sealed override OverviewNodeState State { get; set; }
        
        public string Value { get; set; }
    }
}
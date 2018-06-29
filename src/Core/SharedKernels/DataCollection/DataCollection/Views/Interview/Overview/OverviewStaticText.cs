using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewStaticText : OverviewNode
    {
        public OverviewStaticText(InterviewTreeStaticText treeNode, IStatefulInterview interview) : base(treeNode)
        {
            if (treeNode.FailedErrors.Count > 0)
            {
                this.State = OverviewNodeState.Invalid;
            }
            else
            {
                this.State = OverviewNodeState.Answered;
            }

            this.ErrorMessages = interview.GetFailedValidationMessages(treeNode.Identity, "")
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
            this.HasWarnings = interview.GetFailedWarningMessages(treeNode.Identity, "").Any();
        }

        public List<string> ErrorMessages { get; set; }

        public sealed override OverviewNodeState State { get; set; }

        public bool HasWarnings { get; set; }
    }
}

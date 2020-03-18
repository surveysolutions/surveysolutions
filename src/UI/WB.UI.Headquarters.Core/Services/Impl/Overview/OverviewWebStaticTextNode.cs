using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebStaticTextNode : OverviewStaticText
    {
        public OverviewWebStaticTextNode(InterviewTreeStaticText treeNode, IStatefulInterview interview) : base(treeNode, interview)
        {
        }

        public string AttachmentContentId { get; set; }
    }
}

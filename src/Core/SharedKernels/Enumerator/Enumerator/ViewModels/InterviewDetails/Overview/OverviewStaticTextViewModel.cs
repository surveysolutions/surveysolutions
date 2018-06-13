using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewStaticTextViewModel : OverviewStaticText
    {
        public OverviewStaticTextViewModel(InterviewTreeStaticText treeNode,
            AttachmentViewModel attachmentViewModel) : base(treeNode)
        {
            this.Attachment = attachmentViewModel;
            this.Attachment.Init(treeNode.Tree.InterviewId, treeNode.Identity);
        }

        public AttachmentViewModel Attachment { get; set; }
    }
}